using System.Text;
using System.Text.RegularExpressions;

namespace DapperSqlConstructor.Models
{
    /// <summary>
    /// Describes metadata about a SQL tables and their's corresponding C# models.
    /// Stores parsed table definition, related class name, column-to-property mappings, and foreign key references.
    /// </summary>
    public class DapperMethodBuilder
    {
        /// <summary>
        /// Character used to mark input values
        /// </summary>
        private char PrefixValueChar { get; set; }

        /// <summary>
        /// String with scripts for table
        /// </summary>
        public string TableScriptString { get; set; }

        /// <summary>
        /// String with model which mapped for table
        /// </summary>
        public string MappedClassesString { get; set; }

        /// <summary>
        /// Info about tables and related classes
        /// </summary>
        public List<MappedTableModel> MappedTables { get; set; }

        /// <summary>
        /// Queue with the sequence of table names used in the SQL script
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

        #region Regex patterns

        /// <summary>
        /// Pattern get all table part from script
        /// </summary>
        private readonly Regex tableKeyWordPart = new Regex("table|[)]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Pattern analyze table from script
        /// </summary>
        private readonly Regex tablePart = new Regex("table\\s[a-zA-Z0-9_]+\\s", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Pattern which analyze Foreign key
        /// </summary>
        private readonly Regex foreignKeyPart = new Regex("FOREIGN\\sKEY\\s*\\(\\s*(.*?)\\s*\\).*?REFERENCES\\s*?(.*?)\\s*?\\(\\s*?(.*?)\\s*?\\)",
                                               RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Pattern which analyze data for classes
        /// </summary>
        private readonly Regex commentClassPart = new Regex("\\(\\s*?Table:\\s*?(.*?)\\).*?public class (\\w+)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Pattern which analyze data for properties
        /// </summary>
        private readonly Regex commentPropertyPart = new Regex("\\(\\s*?Column:\\s*?(.*?)\\).*?public\\s*?[\\w\\?<>]+\\s*?(\\w+)\\s*?{\\s*?get;\\s*?set;\\s*?}",
                                             RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Pattern properties part which describe it
        /// </summary>
        private readonly Regex propertiesPartRegex = new Regex("///\\s*?<summary>.*?}\\s*}", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Pattern table part in script
        /// </summary>
        private readonly Regex tablesPartsRegex = new Regex("table.*?[)];", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        #endregion

        /// <summary>
        /// Parse scripts for tables and get general info.
        /// </summary>
        private void ParseTableScripts()
        {
            //Matches only tables parts of script
            var matches = tablesPartsRegex.Matches(TableScriptString);

            if (matches.Any())
            {
                string[] keyWords = new string[] { "CONSTRAINT", "FOREIGN", "REFERENCES", "ON", "PRIMARY", "DELETE", "CASCADE", "UNIQUE" };

                //Loop tables matches
                foreach (Match match in matches)
                {
                    if(match == null || String.IsNullOrEmpty(match.Value))
                        continue;

                    var genTable = new MappedTableModel()
                    {
                        Columns = new Dictionary<string, string>()
                    };

                    //Get text for only table
                    var tableMatch = tablePart.Match(match.Value);

                    if(tableMatch == null || String.IsNullOrEmpty(tableMatch.Value))
                        continue;

                    genTable.TableName = tableKeyWordPart.Replace(tableMatch.Value, "", 1).Trim();

                    //find start and end columns part in table script 
                    var startPoint = match.Value.IndexOf("(");
                    var endPoint = match.Value.IndexOf(");");

                    if (startPoint == -1 || endPoint == -1 || startPoint + 1 >= endPoint)
                        continue;

                    //Get columns for tables
                    var listColumn = match.Value.Substring(startPoint + 1, endPoint - startPoint - 1).Replace("\r", " ").Replace("\t", " ")
                                           .Split("\n").Where(x => !String.IsNullOrWhiteSpace(x));

                    foreach (var colData in listColumn)
                    {
                        var colName = colData.Trim().Split(' ')[0];

                        if (!String.IsNullOrEmpty(colName) && !keyWords.Any(x => x == colName.ToUpper()))
                        {
                            //Set columns data
                            genTable.Columns.Add(colName, String.Empty);
                        }
                    }

                    //Extract foreign key relationships from table sctipt and build reference mappings.
                    var foreignKeyMatches = foreignKeyPart.Matches(match.Value);

                    if (foreignKeyMatches.Any())
                    {
                        genTable.ReferencesTables = new List<RelatedTableModel>();

                        //Loop foreign key parts
                        foreach (Match foreignKeyMatch in foreignKeyMatches)
                        {
                            if (foreignKeyMatch.Groups.Count < 3)
                                continue;

                            var linkedTableInfo = new RelatedTableModel();

                            var fkStr = foreignKeyMatch.Groups[1]?.Value;
                            var refTableStr = foreignKeyMatch.Groups[2]?.Value;
                            var refColumnsStr = foreignKeyMatch.Groups[3]?.Value;

                            var listFk = fkStr.Split(',').Select(x => x.Trim()).ToList();
                            var listRef = refColumnsStr.Split(',').Select(x => x.Trim()).ToList();

                            if (listFk.Count == listRef.Count)
                            {
                                linkedTableInfo.RelatedTable = refTableStr.Trim();
                                linkedTableInfo.ForeignKeyColumns = new Dictionary<string, string>();

                                for (int i = 0; i < listFk.Count(); i++)
                                {
                                    linkedTableInfo.ForeignKeyColumns.Add(listFk[i], listRef[i]);
                                }
                            }

                            genTable.ReferencesTables.Add(linkedTableInfo);
                        }
                    }

                    MappedTables.Add(genTable);
                }
            }
        }

        /// <summary>
        /// Parse models for tables and matches properties for columns
        /// </summary>
        private void ParseModelString()
        {
            var matches = propertiesPartRegex.Matches(MappedClassesString);

            if (matches.Any())
            {
                foreach (Match match in matches)
                {
                    var tableDataStr = commentClassPart.Match(match.Value);
                    var propertiesDataStr = commentPropertyPart.Matches(match.Value);

                    if (tableDataStr.Groups.Count <= 2)
                        continue;

                    // Get related data of table from comment
                    var tableName = tableDataStr.Groups[1].Value?.Trim();
                    var className = tableDataStr.Groups[2].Value?.Trim();

                    var dTable = MappedTables.FirstOrDefault(x => x.TableName == tableName);

                    if (dTable == null)
                        continue;

                    dTable.RelatedClass = className;

                    foreach (Match propertyDataStr in propertiesDataStr)
                    {
                        if (propertyDataStr.Groups.Count <= 2)
                            continue;

                        // Get related data of column from comment
                        var colRelated = propertyDataStr.Groups[1].Value?.Trim();
                        var propName = propertyDataStr.Groups[2].Value?.Trim();

                        if (dTable.Columns.ContainsKey(colRelated))
                            dTable.Columns[colRelated] = propName;
                    }
                }
            }
        }

        /// <summary>
        /// This method construct select request through all tables
        /// </summary>
        private void ConstructSqlSelectRequest()
         {
            var indexTable = 1;
            var parentAliasTable = $"t{indexTable}";

            StringBuilder sqlScript = new StringBuilder("SELECT ");

            var mainTable = MappedTables.FirstOrDefault(x => x.ReferencesTables == null);

            if(mainTable != null)
            {
                var joinPart = new StringBuilder();

                _queueSelectTables = new Queue<string>();
                _queueSelectTables.Enqueue(mainTable.TableName);

                var selectMethodParts = mainTable.Columns.Where(x => !String.IsNullOrEmpty(x.Key) && !String.IsNullOrEmpty(x.Value))
                                                        .Select(x => $" {parentAliasTable}.{x.Key} AS {{nameof({mainTable.RelatedClass}.{x.Value})}}").ToList();
                var simpleSelectSelectParts = mainTable.Columns.Select(x => $" {parentAliasTable}.{x.Key} ").ToList();

                var childTables = MappedTables.Where(x => x.ReferencesTables != null && x.ReferencesTables.Any(y => y.RelatedTable == mainTable.TableName));

                if (childTables.Any())
                {
                    ConstructChildTableScript(childTables, mainTable.TableName, parentAliasTable, joinPart, selectMethodParts, simpleSelectSelectParts, ref indexTable);
                }

                _sqlSelectRequestMethod = new StringBuilder("SELECT ").AppendLine(string.Join(",\n", selectMethodParts)).Append($" FROM {mainTable.TableName} {parentAliasTable} ")
                                         .AppendLine(joinPart.ToString()).ToString();
                _sqlSelectRequestSimple = new StringBuilder("SELECT ").AppendLine(string.Join(",\n", simpleSelectSelectParts)).Append($" FROM {mainTable.TableName} {parentAliasTable} ")
                                         .AppendLine(joinPart.ToString()).ToString();
            }
        }

        /// <summary>
        /// Add elements for select request, add elements from related table, joining them
        /// </summary>
        /// <param name="childTables">Related table data</param>
        /// <param name="parentTName">Parent table name</param>
        /// <param name="parentAliasTable">Parent table alias</param>
        /// <param name="joinPart">Join part of request</param>
        /// <param name="selectMethodParts">Select part of request</param>
        /// <param name="simpleSelectSelectParts">Select part of request (only for select script)</param>
        /// <param name="indexTable">Number of table in sequences</param>
        private void ConstructChildTableScript(IEnumerable<MappedTableModel> childTables, string parentTName, string parentAliasTable, StringBuilder joinPart, 
                                               List<string> selectMethodParts, List<string> simpleSelectSelectParts, ref int indexTable)
        {
            foreach (var childTable in childTables)
            {
                indexTable++;
                var childTableAlias = $"t{indexTable}";

                var referenceTable = childTable.ReferencesTables.FirstOrDefault(x => x.RelatedTable == parentTName);
                var joinStr = new StringBuilder($" LEFT JOIN {childTable.TableName} {childTableAlias} ON ");
                var joinCondition = new StringBuilder();

                _queueSelectTables.Enqueue(childTable.TableName);

                foreach (var fk in referenceTable.ForeignKeyColumns)
                {
                    if (joinCondition.Length > 0)
                        joinCondition.Append($" AND {childTableAlias}.{fk.Key} = {parentAliasTable}.{fk.Value}");
                    else
                        joinCondition.Append($"{childTableAlias}.{fk.Key} = {parentAliasTable}.{fk.Value}");
                }

                joinPart.AppendLine(joinStr.ToString()).Append(joinCondition);

                selectMethodParts.AddRange(childTable.Columns.Where(x => !String.IsNullOrEmpty(x.Key) && !String.IsNullOrEmpty(x.Value))
                                                    .Select(x => $" {parentAliasTable}.{x.Key} AS {{nameof({childTable.RelatedClass}.{x.Value})}}"));
                simpleSelectSelectParts.AddRange(childTable.Columns.Select(x => $" {parentAliasTable}.{x.Key} "));

                var nextChildTables = MappedTables.Where(x => x.ReferencesTables != null && x.ReferencesTables.Any(y => y.RelatedTable == childTable.TableName));

                if(nextChildTables.Any())
                {
                    ConstructChildTableScript(nextChildTables, childTable.TableName, childTableAlias, joinPart, selectMethodParts, simpleSelectSelectParts, ref indexTable);
                }
            }    
        }

        /// <summary>
        /// Generates an SELECT method for each mapped table and its model using Dapper syntax.
        /// </summary>
        private void ConstructDapperSelectMethod()
        {
            var mainTable = MappedTables.FirstOrDefault(x => x.ReferencesTables == null);

            if(mainTable != null)
            {
                var generalTypeStr = new StringBuilder();
                var argumentsFuncPart = new StringBuilder();
                var splitStr = new StringBuilder();

                int tableIdx = 1;
                foreach (var table in _queueSelectTables)
                {
                    var tableData = MappedTables.FirstOrDefault(x => x.TableName == table);

                    generalTypeStr.Append($"{tableData.RelatedClass}, ");

                    argumentsFuncPart.Append($"obj{tableIdx}");

                    var firstColumn = tableData.Columns.First();
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
        }

        /// <summary>
        /// Generates an INSERT method for each mapped table and its model using Dapper syntax.
        /// </summary>
        private void ConstructDapperInsertMethod()
        {
            foreach(var dataInfo in MappedTables)
            {
                var sqlInsCol = new StringBuilder($"INSERT INTO {dataInfo.TableName} ( ");
                var sqlInsVal = new StringBuilder("VALUES (");

                var lastItem = dataInfo.Columns.Last();

                foreach (var colomn in dataInfo.Columns.Where(x => !String.IsNullOrEmpty(x.Key) && !String.IsNullOrEmpty(x.Value)))
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
   var sql = @$""{sqlInsCol.ToString()} {sqlInsVal.ToString()}"";

   using var connection = new SqlConnection(_connectionString);
   await connection.OpenAsync();
   
   await connection.ExecuteAsync(sql, item);
}}";
            }
        }

        /// <summary>
        /// Generates an UPDATE method for each mapped table and its model using Dapper syntax.
        /// </summary>
        private void ConstructDapperUpdateMethod()
        {
            foreach (var dataInfo in MappedTables)
            {
                var sqlStr = new StringBuilder($"UPDATE {dataInfo.TableName} ");

                var firstItem = dataInfo.Columns.First();
                var lastItem = dataInfo.Columns.Last();

                foreach (var colomn in dataInfo.Columns.Where(x => !String.IsNullOrEmpty(x.Key) && !String.IsNullOrEmpty(x.Value)))
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

        /// <summary>
        /// Constract SELECT, INSERT, UPDATE methods
        /// </summary>
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
    /// Class which describes information about table and mapped model. This class consist disassembled information for scripts for table and class.
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
        public Dictionary<string, string> Columns { get; set; }

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
    /// Object describes information about references table, columns which is foreighn keys.
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
        public Dictionary<string, string> ForeignKeyColumns { get; set; }
    }
}
