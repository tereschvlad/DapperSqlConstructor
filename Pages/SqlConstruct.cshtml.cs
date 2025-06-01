using System.Text.RegularExpressions;
using DapperSqlConstructor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DapperSqlConstructor.Pages
{
    public class SqlConstructModel : PageModel
    {
        public void OnGet()
        {
            SqlScripts = @"CREATE TABLE RISK_REP_KO ( 
	ID NUMBER(20,0),
	CRT_DATE DATE DEFAULT SYSDATE,
	REP_YEAR NUMBER(4,0),
	
	DOC_CODE VARCHAR2(17),
	REG_NUM VARCHAR2(10),
	DEADLINE DATE,
	C_STI_DOC NUMBER(4,0),
	C_DOC VARCHAR2(3),
	C_DOC_SUB VARCHAR2(3),
	C_DOC_VER NUMBER(2,0),
	C_STI_MAIN NUMBER(4,0),
	REG_DATE DATE,
	FLAGS NUMBER,
	RISK_TAXPAYER_ID NUMBER(20,0)
);


CREATE TABLE RISK_REP_KO_OPER (
	ID NUMBER(20,0),
	CRT_DATE DATE DEFAULT SYSDATE,
	REP_YEAR NUMBER(4,0),
	
	KO_FULLNAME	VARCHAR2(500),
	EDRPOU_NRESID VARCHAR2(100),
	COUNTRY_CODE NUMBER(3,0),
	
	REASON_CODE1 VARCHAR2(3),
	REASON_CODE2 VARCHAR2(3),
	REASON_CODE3 VARCHAR2(3),
	REASON_CODE4 VARCHAR2(3),
	OPERATION_CODE VARCHAR2(10),
	
	RISK_REP_KO_ID NUMBER(20,0),
	CONSTRAINT fk_department
        FOREIGN KEY (RISK_REP_KO_ID)
        REFERENCES RISK_REP_KO(ID)
);
";

            ConnectedClasses = @"/// <summary>
    /// RISK_REP_KO
    /// </summary>
    public class RiskReportKoDataModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// CRT_DATE
        /// </summary>
        public DateTime? CrtDate { get; set; }

        /// <summary>
        /// REP_YEAR
        /// </summary>
        public int PerYear { get; set; }

        /// <summary>
        /// DOC_CODE
        /// </summary>
        public string DocumentCode { get; set; }

        /// <summary>
        /// REG_NUM
        /// </summary>
        public string RegNum { get; set; }

        /// <summary>
        /// DEADLINE
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// C_STI_DOC
        /// </summary>
        public int CStiDoc { get; set; }

        /// <summary>
        /// C_DOC
        /// </summary>
        public string CDoc { get; set; }

        /// <summary>
        /// C_DOC_SUB
        /// </summary>
        public string CDocSub { get; set; }

        /// <summary>
        /// C_DOC_VER
        /// </summary>
        public int CDocVersion { get; set; }

        /// <summary>
        /// C_STI_MAIN
        /// </summary>
        public int CStiMain { get; set; }

        /// <summary>
        /// REG_DATE
        /// </summary>
        public DateTime? RegDate { get; set; }

        /// <summary>
        ///FLAGS
        /// </summary>
        public long Flags { get; set; }

        /// <summary>
        /// IS_LOAD_OPERS
        /// </summary>
        public bool IsLoadOpers { get; set; }

        /// <summary>
        /// IS_LOAD_OPERS
        /// </summary>
        public List<RiskKoOperDataModel> DataList { get; set; }
    }

    /// <summary>
    /// RISK_REP_KO_OPER
    /// </summary>
    public class RiskKoOperDataModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// EDRPOU
        /// </summary>
        public string Edrpou { get; set; }

        /// <summary>
        /// CREG
        /// </summary>
        public int CReg { get; set; }

        /// <summary>
        /// KO_FULLNAME
        /// </summary>
        public string KoFullname { get; set; }

        /// <summary>
        /// EDRPOU_NRESID
        /// </summary>
        public string EdrpouNResid { get; set; }

        /// <summary>
        /// COUNTRY_CODE
        /// </summary>
        public int CountryCode { get; set; }

        /// <summary>
        /// REASON_CODE1
        /// </summary>
        public string ReasonCode1 { get; set; }

        /// <summary>
        /// REASON_CODE2
        /// </summary>
        public string ReasonCode2 { get; set; }

        /// <summary>
        /// REASON_CODE3
        /// </summary>
        public string ReasonCode3 { get; set; }
    }";

            SqlScripts = @"CREATE TABLE PARENT_TABLE (
    PARENT_ID          NUMBER PRIMARY KEY,
    PARENT_NAME        VARCHAR2(100),
    DESCRIPTION        VARCHAR2(255),
    CREATED_AT         DATE DEFAULT SYSDATE,
    STATUS             VARCHAR2(20),
    IS_ACTIVE          CHAR(1),
    CREATED_BY         VARCHAR2(50),
    UPDATED_AT         DATE,
    UPDATED_BY         VARCHAR2(50),
    REMARKS            VARCHAR2(255)
);

CREATE TABLE CHILD_TABLE (
    CHILD_ID           NUMBER PRIMARY KEY,
    PARENT_ID          NUMBER,
    CHILD_NAME         VARCHAR2(100),
    DESCRIPTION        VARCHAR2(255),
    CREATED_AT         DATE DEFAULT SYSDATE,
    STATUS             VARCHAR2(20),
    IS_ACTIVE          CHAR(1),
    CREATED_BY         VARCHAR2(50),
    UPDATED_AT         DATE,
    UPDATED_BY         VARCHAR2(50),
    CONSTRAINT FK_PARENT
        FOREIGN KEY (PARENT_ID)
        REFERENCES PARENT_TABLE(PARENT_ID)
        ON DELETE CASCADE
);

CREATE TABLE GRANDCHILD_TABLE (
    GRANDCHILD_ID      NUMBER PRIMARY KEY,
    CHILD_ID           NUMBER,
    GRANDCHILD_NAME    VARCHAR2(100),
    DESCRIPTION        VARCHAR2(255),
    CREATED_AT         DATE DEFAULT SYSDATE,
    STATUS             VARCHAR2(20),
    IS_ACTIVE          CHAR(1),
    CREATED_BY         VARCHAR2(50),
    UPDATED_AT         DATE,
    UPDATED_BY         VARCHAR2(50),
    CONSTRAINT FK_CHILD
        FOREIGN KEY (CHILD_ID)
        REFERENCES CHILD_TABLE(CHILD_ID)
        ON DELETE CASCADE
);
";

            ConnectedClasses = @"/// <summary>
/// PARENT_TABLE
/// </summary>
public class ParentTable
{
    /// <summary>
    /// PARENT_ID
    /// </summary>
    public int parentId { get; set; }

    /// <summary>
    /// PARENT_NAME
    /// </summary>
    public string parentName { get; set; }

    /// <summary>
    /// DESCRIPTION
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// CREATED_AT
    /// </summary>
    public DateTime createdAt { get; set; }

    /// <summary>
    /// STATUS
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// IS_ACTIVE
    /// </summary>
    public string isActive { get; set; }

    /// <summary>
    /// CREATED_BY
    /// </summary>
    public string createdBy { get; set; }

    /// <summary>
    /// UPDATED_AT
    /// </summary>
    public DateTime? updatedAt { get; set; }

    /// <summary>
    /// UPDATED_BY
    /// </summary>
    public string updatedBy { get; set; }

    /// <summary>
    /// REMARKS
    /// </summary>
    public string remarks { get; set; }

    /// <summary>
    /// CHILD_TABLE 
    /// </summary>
    public List<ChildTable> children { get; set; }
}

/// <summary>
/// CHILD_TABLE
/// </summary>
public class ChildTable
{
    /// <summary>
    /// CHILD_ID
    /// </summary>
    public int childId { get; set; }

    /// <summary>
    /// PARENT_ID
    /// </summary>
    public int parentId { get; set; }

    /// <summary>
    /// CHILD_NAME
    /// </summary>
    public string childName { get; set; }

    /// <summary>
    /// DESCRIPTION
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// CREATED_AT
    /// </summary>
    public DateTime createdAt { get; set; }

    /// <summary>
    /// STATUS
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// IS_ACTIVE
    /// </summary>
    public string isActive { get; set; }

    /// <summary>
    /// CREATED_BY
    /// </summary>
    public string createdBy { get; set; }

    /// <summary>
    /// UPDATED_AT
    /// </summary>
    public DateTime? updatedAt { get; set; }

    /// <summary>
    /// UPDATED_BY
    /// </summary>
    public string updatedBy { get; set; }

    /// <summary>
    /// GRANDCHILD_TABLE
    /// </summary>
    public List<GrandchildTable> grandchildren { get; set; }
}


/// <summary>
/// GRANDCHILD_TABLE
/// </summary>
public class GrandchildTable
{
    /// <summary>
    /// GRANDCHILD_ID
    /// </summary>
    public int grandchildId { get; set; }

    /// <summary>
    /// CHILD_ID
    /// </summary>
    public int childId { get; set; }

    /// <summary>
    /// DETAIL
    /// </summary>
    public string detail { get; set; }

    /// <summary>
    /// CREATED_AT
    /// </summary>
    public DateTime createdAt { get; set; }

    /// <summary>
    /// STATUS
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// IS_ACTIVE
    /// </summary>
    public string isActive { get; set; }

    /// <summary>
    /// CREATED_BY
    /// </summary>
    public string createdBy { get; set; }

    /// <summary>
    /// UPDATED_AT
    /// </summary>
    public DateTime? updatedAt { get; set; }

    /// <summary>
    /// UPDATED_BY
    /// </summary>
    public string updatedBy { get; set; }
}";

            //var tableData = ConstructDataInfo();
            //tableData = ConstructClassesInfo(tableData);

            var builder = new DapperMethodBuilder(SqlScripts, ConnectedClasses);
            builder.ParseTableProperties();
        }

        [BindProperty]
        public string SqlScripts { get; set; }

        [BindProperty]
        public string ConnectedClasses { get; set; }


        public async Task<IActionResult> OnPostAsync()
        {
            

            return RedirectToPage("./Index");
        }

        public List<GeneralTableStructure> ConstructDataInfo()
        {
            var matches = Regex.Matches(SqlScripts, "table.*?[)];", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var tableRegex = new Regex("table|[)]", RegexOptions.IgnoreCase);

            string[] keyWords = new string[] { "CONSTRAINT", "FOREIGN", "REFERENCES" };
            var tablesData = new List<GeneralTableStructure>();

            if (matches.Any())
            {
                foreach (var match in matches.ToList())
                {
                    if (match != null && !String.IsNullOrEmpty(match.Value))
                    {
                        var genTable = new GeneralTableStructure()
                        {
                            Colums = new Dictionary<string, string>()
                        };

                        var tableMatch = Regex.Match(match.Value, "table\\s[a-zA-Z0-9_]+\\s", RegexOptions.IgnoreCase);
                        genTable.TableName = tableRegex.Replace(tableMatch.Value, "").Trim();

                        var startPoint = match.Value.IndexOf("(");
                        var endPoint = match.Value.IndexOf(");");

                        var columnsData = match.Value.Substring(startPoint + 1, endPoint - startPoint - 1).Replace("\r", " ").Replace("\t", " ");

                        var listColumn = columnsData.Split("\n").Where(x => !String.IsNullOrWhiteSpace(x));

                        foreach (var colData in listColumn)
                        {
                            var colName = colData.Trim().Split(' ')[0];

                            if (!String.IsNullOrEmpty(colName) && !keyWords.Any(x => x == colName.ToUpper()))
                            {
                                genTable.Colums.Add(colName, String.Empty);
                            }
                        }

                        var foreignKeyStr = Regex.Match(match.Value, "FOREIGN\\s+KEY.*\\)", RegexOptions.IgnoreCase).Value;
                        var refencesStr = Regex.Match(match.Value, "REFERENCES.*\\)", RegexOptions.IgnoreCase).Value;

                        if (!String.IsNullOrWhiteSpace(foreignKeyStr) && !String.IsNullOrWhiteSpace(refencesStr))
                        {
                            var fkStr = Regex.Replace(foreignKeyStr, "FOREIGN\\sKEY\\s*[(]|[)]", "", RegexOptions.IgnoreCase);
                            var refStr = Regex.Replace(refencesStr, "REFERENCES.*\\s*[(]|[)]", "", RegexOptions.IgnoreCase);
                            var refTableStr = Regex.Replace(refencesStr, "REFERENCES\\s*|[(].*", "", RegexOptions.IgnoreCase);

                            var listFk = fkStr.Split(',').Select(x => x.Trim()).ToList();
                            var listRef = refStr.Split(',').Select(x => x.Trim()).ToList();

                            if (listFk.Count == listRef.Count)
                            {
                                genTable.ReferenceTableName = refTableStr;
                                genTable.ReferencesColumn = new Dictionary<string, string>();

                                for (int i = 0; i < listFk.Count(); i++)
                                {
                                    genTable.ReferencesColumn.Add(listFk[i], listRef[i]);
                                }
                            }
                        }

                        tablesData.Add(genTable);

                    }

                }
            }

            return tablesData;
        }

        public List<GeneralTableStructure> ConstructClassesInfo(List<GeneralTableStructure> tableData)
        {
            var matches = Regex.Matches(ConnectedClasses, "///\\s*?<summary>.*?}\\s*}", RegexOptions.IgnoreCase | RegexOptions.Singleline);

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

                        var dTable = tableData.FirstOrDefault(x => x.TableName == tableName);

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

            return tableData;
        }
    }

	public class GeneralTableStructure
	{
        public string TableName { get; set; }

        public string RelatedClass { get; set; }

        public Dictionary<string, string> Colums { get; set; }

        public string ReferenceTableName { get; set; }
        public Dictionary<string, string> ReferencesColumn { get; set; }
    }
}
