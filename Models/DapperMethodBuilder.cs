using System.Text;
using System.Text.RegularExpressions;

namespace DapperSqlConstructor.Models
{
    /// <summary>
    /// Class has a functionality for constructing select, insert and update request dapper method for tables and models wich mepped for them.
    /// </summary>
    public class DapperMethodBuilder
    {
        /// <summary>
        /// Chart wich marks input value
        /// </summary>
        private char PrefixValueChar { get; set; }

        /// <summary>
        /// String with scripts for table
        /// </summary>
        public string TableScriptString { get; set; }

        /// <summary>
        /// String with model wich mapped for table
        /// </summary>
        public string MappedClassesString { get; set; }

        /// <summary>
        /// Info about tables and related classes
        /// </summary>
        public List<MappedTableModel> MappedTables { get; set; }

        /// <summary>
        /// Queue where writed sequence tables in sql script
        /// </summary>

        private Queue<string> _queueSelectTables;


        private string _sqlSelectRequestMethod;

        /// <summary>
        /// Includes select sql for method request with mapping part
        /// </summary>
        public string SqlSelectRequestMethod => _sqlSelectRequestMethod;


        private string _sqlSelectRequestSimple;

        /// <summary>
        /// Includes simple sql request with mapping part
        /// </summary>
        public string SqlSelectRequestSimple => _sqlSelectRequestSimple;


        private string _sqlSelectMethod;

        /// <summary>
        /// Includes describe select method, through all tables 
        /// </summary>
        public string SqlSelectMethod => _sqlSelectMethod;

        public DapperMethodBuilder(string createdTableScript, string mappedClasses, char prefixValue = ':')
        {
            TableScriptString = createdTableScript;
            MappedClassesString = mappedClasses;
            MappedTables = new List<MappedTableModel>();
            PrefixValueChar = prefixValue;
        }

        /// <summary>
        /// Parse scripts for tables and get general info.
        /// </summary>
        public void ParseTableScripts()
        {
            //Matches only tables parts of script
            var matches = Regex.Matches(TableScriptString, "table.*?[)];", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (matches.Any())
            {
                //Initialise regex patterns
                var tableKeyWordPart = new Regex("table|[)]", RegexOptions.IgnoreCase);
                var tablePart = new Regex("table\\s[a-zA-Z0-9_]+\\s", RegexOptions.IgnoreCase);
                var foreignKeyPart = new Regex("FOREIGN\\sKEY\\s*\\(\\s*(.*?)\\s*\\).*?REFERENCES\\s*?(.*?)\\s*?\\(\\s*?(.*?)\\s*?\\)",
                                               RegexOptions.IgnoreCase | RegexOptions.Singleline);

                string[] keyWords = new string[] { "CONSTRAINT", "FOREIGN", "REFERENCES", "ON", "PRIMARY" };

                //Loop tables matches
                foreach (var match in matches.ToList())
                {
                    if (match != null && !String.IsNullOrEmpty(match.Value))
                    {
                        var genTable = new MappedTableModel()
                        {
                            Colums = new Dictionary<string, string>()
                        };

                        //Get text for only table
                        var tableMatch = tablePart.Match(match.Value);
                        genTable.TableName = tableKeyWordPart.Replace(tableMatch.Value, "", 1).Trim();

                        var startPoint = match.Value.IndexOf("(");
                        var endPoint = match.Value.IndexOf(");");

                        //Get colums for tables
                        var listColumn = match.Value.Substring(startPoint + 1, endPoint - startPoint - 1).Replace("\r", " ").Replace("\t", " ")
                                               .Split("\n").Where(x => !String.IsNullOrWhiteSpace(x));

                        foreach (var colData in listColumn)
                        {
                            var colName = colData.Trim().Split(' ')[0];

                            if (!String.IsNullOrEmpty(colName) && !keyWords.Any(x => x == colName.ToUpper()))
                            {
                                //Set columns data
                                genTable.Colums.Add(colName, String.Empty);
                            }
                        }

                        //Get data about foreign keyses
                        var foreighKeyMatches = foreignKeyPart.Matches(match.Value);

                        if(foreighKeyMatches.Any())
                        {
                            genTable.ReferencesTables = new List<RelatedTableModel>();

                            foreach (var foreighKeyMatch in foreighKeyMatches.ToList())
                            {
                                if (foreighKeyMatch.Groups.Count >= 3)
                                {
                                    var linkedTableInfo = new RelatedTableModel();

                                    var fkStr = foreighKeyMatch.Groups[1]?.Value;
                                    var refTableStr = foreighKeyMatch.Groups[2]?.Value;
                                    var refColumsStr = foreighKeyMatch.Groups[3]?.Value;

                                    var listFk = fkStr.Split(',').Select(x => x.Trim()).ToList();
                                    var listRef = refColumsStr.Split(',').Select(x => x.Trim()).ToList();

                                    if (listFk.Count == listRef.Count)
                                    {
                                        linkedTableInfo.RelatedTable = refTableStr.Trim();
                                        linkedTableInfo.ForeighnKeyColumns = new Dictionary<string, string>();

                                        for (int i = 0; i < listFk.Count(); i++)
                                        {
                                            linkedTableInfo.ForeighnKeyColumns.Add(listFk[i], listRef[i]);
                                        }
                                    }

                                    genTable.ReferencesTables.Add(linkedTableInfo);
                                }
                            }
                        }

                        MappedTables.Add(genTable);
                    }
                }
            }
        }

        /// <summary>
        /// Parse models for tables and matches properties for columns
        /// </summary>
        public void ParseModelString()
        {

            var matches = Regex.Matches(MappedClassesString, "///\\s*?<summary>.*?}\\s*}", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (matches.Any())
            {
                //Initialise regex patterns
                var commentClassPart = new Regex("\\(\\s*?Table:\\s*?(.*?)\\).*?public class (\\w+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var commentPropertyPart = new Regex("\\(\\s*?Column:\\s*?(.*?)\\).*?public\\s*?[\\w\\?<>]+\\s*?(\\w+)\\s*?{\\s*?get;\\s*?set;\\s*?}",
                                                     RegexOptions.IgnoreCase | RegexOptions.Singleline);

                foreach (var match in matches.ToList())
                {
                    var tableDataStr = commentClassPart.Match(match.Value);
                    var propertiesDataStr = commentPropertyPart.Matches(match.Value);

                    if (tableDataStr.Groups.Count > 2)
                    {
                        var tableName = tableDataStr.Groups[1].Value?.Trim();
                        var className = tableDataStr.Groups[2].Value?.Trim();

                        var dTable = MappedTables.FirstOrDefault(x => x.TableName == tableName);

                        if (dTable != null)
                        {
                            dTable.RelatedClass = className;

                            foreach (var propertyDataStr in propertiesDataStr.ToList())
                            {
                                if (propertyDataStr.Groups.Count > 2)
                                {
                                    var colRelated = propertyDataStr.Groups[1].Value?.Trim();
                                    var propName = propertyDataStr.Groups[2].Value?.Trim();

                                    if (dTable.Colums.ContainsKey(colRelated))
                                        dTable.Colums[colRelated] = propName;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method construct select request through all tables
        /// </summary>
        public void ConstructSqlSelectRequest()
        {
            var indexTable = 1;
            var parentAliasTable = $"t{indexTable}";

            StringBuilder sqlScript = new StringBuilder("SELECT ");

            var tableData = MappedTables.FirstOrDefault(x => x.ReferencesTables == null);
            var selectMethodPart = new StringBuilder();
            var simpleSelectPart = new StringBuilder();
            var joinPart = new StringBuilder();

            _queueSelectTables = new Queue<string>();
            _queueSelectTables.Enqueue(tableData.TableName);

            foreach (var column in tableData.Colums)
            {
                if(!String.IsNullOrEmpty(column.Key) && !String.IsNullOrEmpty(column.Value))
                    selectMethodPart.AppendLine($" {parentAliasTable}.{column.Key} AS {{nameof({tableData.RelatedClass}.{column.Value})}},");

                simpleSelectPart.AppendLine($" {parentAliasTable}.{column.Key}, ");
            }

            var childTables = MappedTables.Where(x => x.ReferencesTables != null && x.ReferencesTables.Any(y => y.RelatedTable == tableData.TableName));

            if(childTables.Any())
            {
                ConstructChildTableScript(childTables, tableData.TableName, parentAliasTable, joinPart, selectMethodPart, simpleSelectPart, ref indexTable);
            }

            var selectMethodStr = selectMethodPart.ToString();
            var simpleSelectStr = simpleSelectPart.ToString();

            var lastComaSelMethod = selectMethodStr.LastIndexOf(',');
            var lastComaSimpleSelect = simpleSelectStr.LastIndexOf(',');

            _sqlSelectRequestMethod = new StringBuilder("SELECT ").AppendLine(selectMethodStr.Remove(lastComaSelMethod, 1)).Append($" FROM {tableData.TableName} {parentAliasTable} ")
                                     .AppendLine(joinPart.ToString()).ToString();
            _sqlSelectRequestSimple = new StringBuilder("SELECT ").AppendLine(simpleSelectStr.Remove(lastComaSimpleSelect, 1)).Append($" FROM {tableData.TableName} {parentAliasTable} ")
                                     .AppendLine(joinPart.ToString()).ToString();

            simpleSelectPart.Remove(simpleSelectPart.Length - 2, 1);

        }

        /// <summary>
        /// Add elements for select request, add elements from related table, joining them
        /// </summary>
        /// <param name="childTables">Related table data</param>
        /// <param name="parentTName">Parent table name</param>
        /// <param name="parentAliasTable">Parent table alias</param>
        /// <param name="indexTable">Number of table in sequences</param>
        /// <param name="joinPart">Join part of request</param>
        /// <param name="selectMethodPart">Select part of request</param>
        private void ConstructChildTableScript(IEnumerable<MappedTableModel> childTables, string parentTName, string parentAliasTable, StringBuilder joinPart, 
                                               StringBuilder selectMethodPart, StringBuilder simpleSelectPart, ref int indexTable)
        {
            foreach (var childTable in childTables)
            {
                indexTable++;
                var childNTable = $"t{indexTable}";

                var referenceTable = childTable.ReferencesTables.FirstOrDefault(x => x.RelatedTable == parentTName);
                var joinStr = new StringBuilder($" LEFT JOIN {childTable.TableName} {childNTable} ON ");
                var joinCondition = new StringBuilder();

                _queueSelectTables.Enqueue(childTable.TableName);

                foreach (var fk in referenceTable.ForeighnKeyColumns)
                {
                    if (joinCondition.Length > 0)
                        joinCondition.Append($" AND {childNTable}.{fk.Key} = {parentAliasTable}.{fk.Value}");
                    else
                        joinCondition.Append($"{childNTable}.{fk.Key} = {parentAliasTable}.{fk.Value}");
                }

                joinPart.AppendLine(joinStr.ToString()).Append(joinCondition);

                foreach (var childColumn in childTable.Colums)
                {
                    if (!String.IsNullOrEmpty(childColumn.Key) && !String.IsNullOrEmpty(childColumn.Value))
                    {
                        selectMethodPart.AppendLine($"{childNTable}.{childColumn.Key} AS {{nameof({childTable.RelatedClass}.{childColumn.Value})}},");
                        simpleSelectPart.AppendLine($"{childNTable}.{childColumn.Key},");
                    }
                }

                var nextChildTables = MappedTables.Where(x => x.ReferencesTables != null && x.ReferencesTables.Any(y => y.RelatedTable == childTable.TableName));

                if(nextChildTables.Any())
                {
                    ConstructChildTableScript(nextChildTables, childTable.TableName, childNTable, joinPart, selectMethodPart, simpleSelectPart, ref indexTable);
                }
            }    
        }

        /// <summary>
        /// Construct select dapper method
        /// </summary>
        private void ConstructDapperSelectMethod()
        {
            var mainTable = MappedTables.FirstOrDefault(x => x.ReferencesTables == null);

            var generalTypeStr = new StringBuilder();
            var argumentsFuncPart = new StringBuilder();
            var splitStr = new StringBuilder();

            int tableIdx = 1;
            foreach (var table in _queueSelectTables)
            {
                var tableData = MappedTables.FirstOrDefault(x => x.TableName == table);

                generalTypeStr.Append($"{tableData.RelatedClass}, ");

                argumentsFuncPart.Append($"obj{tableIdx}");

                var firstColumn = tableData.Colums.First();
                splitStr.Append($"{firstColumn.Value}");

                if (table != _queueSelectTables.Last())
                {
                    argumentsFuncPart.Append(',');
                    splitStr.Append(',');
                }

                tableIdx++;
            }

            var firstQueue = _queueSelectTables.First();
            var firstTableData = MappedTables.FirstOrDefault(x => x.TableName == firstQueue);

            generalTypeStr.Append($"{firstTableData.RelatedClass}");

            _sqlSelectMethod = $@"
public async Task<IEnumerable<{mainTable.RelatedClass}>> Get{mainTable.RelatedClass}List()
{{
      using var connection = new SqlConnection(_connectionString);
      var command = @$""{_sqlSelectRequestMethod}"";
      
      return await  connection.QueryAsync<{generalTypeStr.ToString()}>(
      command,
      ({argumentsFuncPart.ToString()}) =>
      {{
          return obj1;
      }},
      splitOn: ""{splitStr.ToString()}"");
}}";

        }

        /// <summary>
        /// Construct insert dapper method, for every table
        /// </summary>
        private void ConstructDapperInsertMethod()
        {
            foreach(var dataInfo in MappedTables)
            {
                var sqlInsCol = new StringBuilder($"INSERT INTO {dataInfo.TableName} ( ");
                var sqlInsVal = new StringBuilder("VALUES (");

                var lastItem = dataInfo.Colums.Last();

                foreach (var colomn in dataInfo.Colums.Where(x => !String.IsNullOrEmpty(x.Key) && !String.IsNullOrEmpty(x.Value)))
                {
                    if(!String.IsNullOrEmpty(colomn.Key) && !String.IsNullOrEmpty(colomn.Value))
                    {
                        sqlInsCol.Append(colomn.Key);
                        sqlInsVal.Append($"{PrefixValueChar}{{nameof({dataInfo.RelatedClass}.{colomn.Value})}}");
                    }

                    if(colomn.Key != lastItem.Key)
                    {
                        sqlInsCol.Append(", ");
                        sqlInsVal.Append(", ");
                    }
                }

                sqlInsCol.Append(")");
                sqlInsVal.Append(")");

                dataInfo.InsertStringMethod = $@"
public async Task Insert{dataInfo.RelatedClass}Async({dataInfo.RelatedClass} item)
{{
   var sql = @$""{sqlInsCol.ToString()}{sqlInsVal.ToString()}"";

   using var connection = new SqlConnection(_connectionString);
   await connection.OpenAsync();
   
   await connection.ExecuteAsync(sql, item);
}}";
            }
        }

        /// <summary>
        /// Construct update dapper method, for every table
        /// </summary>
        private void ConstructDapperUpdateMethod()
        {
            foreach (var dataInfo in MappedTables)
            {
                var sqlStr = new StringBuilder($"UPDATE {dataInfo.TableName} ");

                var firstItem = dataInfo.Colums.First();
                var lastItem = dataInfo.Colums.Last();

                foreach (var colomn in dataInfo.Colums.Where(x => !String.IsNullOrEmpty(x.Key) && !String.IsNullOrEmpty(x.Value)))
                {
                    if (firstItem.Key == colomn.Key)
                    {
                        sqlStr.AppendLine("SET ");
                        continue;
                    }

                    if(!String.IsNullOrEmpty(colomn.Key) && !String.IsNullOrEmpty(colomn.Value))
                        sqlStr.Append($"{colomn.Key} = {PrefixValueChar}{{nameof({dataInfo.RelatedClass}.{colomn.Value})}}");

                    if (lastItem.Key != colomn.Key)
                        sqlStr.AppendLine(", ");
                }

                sqlStr.AppendLine($" WHERE {firstItem.Key} = {PrefixValueChar}{{nameof({dataInfo.RelatedClass}.{firstItem.Value})}}");

                dataInfo.UpdateStringMethod = $@"
public async Task Update{dataInfo.RelatedClass}Async({dataInfo.RelatedClass} item)
{{
   var sql = @$""{sqlStr.ToString()}"";

   using var connection = new SqlConnection(_connectionString);
   await connection.OpenAsync();
   
   await connection.ExecuteAsync(sql, item);
}}";
            }
        }

        public void ParseTableProperties()
        {
            ParseTableScripts();
            ParseModelString();
            ConstructSqlSelectRequest();
            ConstructDapperSelectMethod();
            ConstructDapperInsertMethod();
            ConstructDapperUpdateMethod();
        }
    }

    /// <summary>
    /// Class wich describes information about table and mapped model. This class consist disassembled information for scripts for table and class.
    /// Object has information about mapping properties and columns, mapping information about table and refer class. info about references table etc.
    /// </summary>
    public class MappedTableModel
    {
        /// <summary>
        /// Table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Related class for table
        /// </summary>
        public string RelatedClass { get; set; }

        /// <summary>
        /// Dictionary there exist column and related property
        /// </summary>
        public Dictionary<string, string> Colums { get; set; }

        /// <summary>
        /// Related table information
        /// </summary>
        public List<RelatedTableModel> ReferencesTables { get; set; }

        /// <summary>
        /// Includes describe insert method 
        /// </summary>
        public string InsertStringMethod { get; set; }

        /// <summary>
        /// Includes describe update method 
        /// </summary>
        public string UpdateStringMethod { get; set; }
    }

    /// <summary>
    /// Object describes information about references table, columns wich is foreighn keyses.
    /// </summary>
    public class RelatedTableModel
    {
        /// <summary>
        /// Name of related class
        /// </summary>
        public string RelatedTable { get; set; }

        /// <summary>
        /// Describe for foreighn key
        /// </summary>
        public Dictionary<string, string> ForeighnKeyColumns { get; set; }
    }
}
