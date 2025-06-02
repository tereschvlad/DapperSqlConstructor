using System.Text;
using System.Text.RegularExpressions;

namespace DapperSqlConstructor.Models
{
    public class DapperMethodBuilder
    {
        public char PrefixValueChar { get; set; }
        public string TableScriptString { get; set; }

        public string MappedClassesString { get; set; }

        public List<MappedTableModel> MappedTables { get; set; }

        /// <summary>
        /// Queue where writed sequence tables in sql script
        /// </summary>

        private Queue<string> _queueSelectTables;

        private string _sqlSelectRequest;

        private string _sqlSelectMethod;

        public DapperMethodBuilder(string createdTableScript, string mappedClasses)
        {
            TableScriptString = createdTableScript;
            MappedClassesString = mappedClasses;
            MappedTables = new List<MappedTableModel>();
        }

        /// <summary>
        /// Parse scripts for tables and get general info.
        /// </summary>
        public void ParseTableScripts()
        {
            //Matches only tables parts of script
            var matches = Regex.Matches(TableScriptString, "table.*?[)];", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var tableRegex = new Regex("table|[)]", RegexOptions.IgnoreCase);

            string[] keyWords = new string[] { "CONSTRAINT", "FOREIGN", "REFERENCES" };

            if (matches.Any())
            {
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
                        var tableMatch = Regex.Match(match.Value, "table\\s[a-zA-Z0-9_]+\\s", RegexOptions.IgnoreCase);
                        genTable.TableName = tableRegex.Replace(tableMatch.Value, "", 1).Trim();

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
                        var foreighKeyMatches = Regex.Matches(match.Value, "FOREIGN\\sKEY\\s*\\(\\s*(.*?)\\s*\\).*?REFERENCES\\s*?(.*?)\\s*?\\(\\s*?(.*?)\\s*?\\)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                        if(foreighKeyMatches.Any())
                        {
                            genTable.ReferencesTables = new List<LinkedTableModel>();

                            foreach (var foreighKeyMatch in foreighKeyMatches.ToList())
                            {
                                if (foreighKeyMatch.Groups.Count >= 3)
                                {
                                    var linkedTableInfo = new LinkedTableModel();

                                    var fkStr = foreighKeyMatch.Groups[1]?.Value;
                                    var refTableStr = foreighKeyMatch.Groups[2]?.Value;
                                    var refColumsStr = foreighKeyMatch.Groups[3]?.Value;

                                    var listFk = fkStr.Split(',').Select(x => x.Trim()).ToList();
                                    var listRef = refColumsStr.Split(',').Select(x => x.Trim()).ToList();

                                    if (listFk.Count == listRef.Count)
                                    {
                                        linkedTableInfo.ReferenceTable = refTableStr.Trim();
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
                foreach (var match in matches.ToList())
                {
                    var tableDataStr = Regex.Match(match.Value, "///\\s*?<summary>\\s*///(.*?)///\\s</summary>\\s*public class (\\w+)",
                                                   RegexOptions.IgnoreCase | RegexOptions.Singleline);

                    var propertiesDataStr = Regex.Matches(match.Value, "///\\s*?<summary>\\s*?///\\s*?(\\w+)\\s*?///\\s*?</summary>\\s*?public\\s*?[\\w\\?<>]+\\s*?(\\w+)\\s*?{\\s*?get;\\s*?set;\\s*?}");

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

                                    if (dTable.Colums.ContainsKey(propertyDataStr.Groups[1].Value))
                                        dTable.Colums[colRelated] = propName;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ConstructSqlSelectRequest()
        {
            var indexTable = 1;
            var parentNTable = $"t{indexTable}";

            StringBuilder sqlScript = new StringBuilder("SELECT ");

            var tableData = MappedTables.FirstOrDefault(x => x.ReferencesTables == null);
            var fromPart = new StringBuilder($" FROM {tableData.TableName} {parentNTable} ");

            var selectPart = new StringBuilder();
            var joinPart = new StringBuilder();

            _queueSelectTables = new Queue<string>();
            _queueSelectTables.Enqueue(tableData.TableName);

            foreach (var column in tableData.Colums)
            {
                selectPart.AppendLine($" {parentNTable}.{column.Key} AS {{nameof({tableData.RelatedClass}.{column.Value})}},");
            }

            var childTables = MappedTables.Where(x => x.ReferencesTables != null && x.ReferencesTables.Any(y => y.ReferenceTable == tableData.TableName));

            if(childTables.Any())
            {
                ConstructChildTableScript(childTables, tableData.TableName, parentNTable, ref indexTable, ref fromPart, ref joinPart, ref selectPart);
            }

            selectPart.Remove(selectPart.Length - 2, 1);
            sqlScript.Append(selectPart).Append(fromPart).Append(joinPart);

            _sqlSelectRequest = sqlScript.ToString();
        }

        private void ConstructChildTableScript(IEnumerable<MappedTableModel> childTables, string parentTName, string parentNTable,  ref int indexTable, 
                                               ref StringBuilder fromPart, ref StringBuilder joinPart, ref StringBuilder selectStr)
        {
            foreach (var childTable in childTables)
            {
                indexTable++;
                var childNTable = $"t{indexTable}";

                var referenceTable = childTable.ReferencesTables.FirstOrDefault(x => x.ReferenceTable == parentTName);
                var joinStr = new StringBuilder($" LEFT JOIN {childTable.TableName} ON ");
                var joinCondition = new StringBuilder();

                _queueSelectTables.Enqueue(childTable.TableName);

                foreach (var fk in referenceTable.ForeighnKeyColumns)
                {
                    if (joinCondition.Length > 0)
                        joinCondition.Append($" AND {childNTable}.{fk.Key} = {parentNTable}.{fk.Value}");
                    else
                        joinCondition.Append($"{childNTable}.{fk.Key} = {parentNTable}.{fk.Value}");
                }

                joinPart.Append(joinStr).Append(joinCondition);

                foreach (var childColumn in childTable.Colums)
                {
                    if (!String.IsNullOrEmpty(childColumn.Key) && !String.IsNullOrEmpty(childColumn.Value))
                    {
                        selectStr.AppendLine($"{childNTable}.{childColumn.Key} AS {{nameof({childTable.RelatedClass}.{childColumn.Value})}},");
                    }
                }

                var nextChildTables = MappedTables.Where(x => x.ReferencesTables != null && x.ReferencesTables.Any(y => y.ReferenceTable == childTable.TableName));

                if(nextChildTables.Any())
                {
                    ConstructChildTableScript(nextChildTables, childTable.TableName, childNTable, ref indexTable, ref fromPart, ref joinPart, ref selectStr);
                }
            }    
        }

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
            }

            var firstQueue = _queueSelectTables.First();
            var firstTableData = MappedTables.FirstOrDefault(x => x.TableName == firstQueue);

            generalTypeStr.Append($"{firstTableData.RelatedClass}");

            _sqlSelectMethod = $@"public async Get{mainTable.RelatedClass}List()
                                  {{
                                        using var connection = new SqlConnection(_connectionString);
                                        var command = @$""{_sqlSelectRequest}"";
                                        
                                        return await  connection.QueryAsync<{generalTypeStr.ToString()}>(
                                        command,
                                        ({argumentsFuncPart.ToString()})
                                        {{
                                            return arg1;
                                        }},
                                        splitOn: ""{splitStr.ToString()}""
                                  }}";

        }

        private void ConstructDapperInsertMethod()
        {
            foreach(var dataInfo in MappedTables)
            {
                var sqlInsCol = new StringBuilder($"INSERT INTO {dataInfo.TableName} ( ");
                var sqlInsVal = new StringBuilder("VALUES (");

                var lastItem = dataInfo.Colums.Last();

                foreach (var colomn in dataInfo.Colums)
                {
                    sqlInsCol.Append(colomn.Key);
                    sqlInsVal.Append($"{PrefixValueChar}{{nameof({dataInfo.RelatedClass}.{colomn.Value})}}");

                    if(colomn.Key != lastItem.Key)
                    {
                        sqlInsCol.Append(", ");
                        sqlInsVal.Append(", ");
                    }
                }

                sqlInsCol.Append(")");
                sqlInsVal.Append(")");

                dataInfo.InsertStringMethod = $@"public async Task Insert{dataInfo.RelatedClass}Async({dataInfo.RelatedClass} item)
                                                 {{
                                                    var sql = @$""{sqlInsCol.ToString()}{sqlInsVal.ToString()}"";

                                                    using var connection = new SqlConnection(_connectionString);
                                                    await connection.OpenAsync();
                                                    
                                                    await connection.ExecuteAsync(sql, item);
                                                 }}";
            }
        }

        private void ConstructDapperUpdateMethod()
        {
            foreach (var dataInfo in MappedTables)
            {
                var sqlStr = new StringBuilder($"UPDATE {dataInfo.TableName} SET ");

                var firstItem = dataInfo.Colums.First();
                var lastItem = dataInfo.Colums.Last();

                foreach (var colomn in dataInfo.Colums)
                {
                    if (firstItem.Key == colomn.Key)
                        continue;

                    sqlStr.Append($"{colomn.Key}={{nameof({PrefixValueChar}{dataInfo.RelatedClass}{colomn.Value})}}");

                    if (lastItem.Key == colomn.Key)
                        sqlStr.AppendLine(", ");
                }

                sqlStr.AppendLine($"WHERE {firstItem.Key}={{nameof({PrefixValueChar}{firstItem.Value})}}");

                dataInfo.InsertStringMethod = $@"public async Task Update{dataInfo.RelatedClass}Async({dataInfo.RelatedClass} item)
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

    public class MappedTableModel
    {
        public string TableName { get; set; }

        public string RelatedClass { get; set; }

        public Dictionary<string, string> Colums { get; set; }

        public List<LinkedTableModel> ReferencesTables { get; set; }

        public string InsertStringMethod { get; set; }

        public string UpdateStringMethod { get; set; }
    }

    public class LinkedTableModel
    {
        public string ReferenceTable { get; set; }

        public Dictionary<string, string> ForeighnKeyColumns { get; set; }
    }
}
