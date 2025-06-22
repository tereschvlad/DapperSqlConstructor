using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DapperSqlConstructor.Models;

namespace DapperSqlConstructor.Pages
{
    public class SqlConstructModel : PageModel
    {
        #region Properties

        [BindProperty]
        public string ExampleTableSqlScripts { get; set; } = @"
CREATE TABLE PARENT_TABLE (
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
);";

        [BindProperty]
        public string ExampleClassModels { get; set; } = @"
/// <summary>
/// (Table: PARENT_TABLE)
/// </summary>
public class ParentTable
{
    /// <summary>
    /// (Column: PARENT_ID)
    /// </summary>
    public int parentId { get; set; }

    /// <summary>
    /// (Column: PARENT_NAME)
    /// </summary>
    public string parentName { get; set; }

    /// <summary>
    /// (Column: DESCRIPTION)
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// (Column: CREATED_AT)
    /// </summary>
    public DateTime createdAt { get; set; }

    /// <summary>
    /// (Column: STATUS)
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// (Column: IS_ACTIVE)
    /// </summary>
    public string isActive { get; set; }

    /// <summary>
    /// (Column: CREATED_BY)
    /// </summary>
    public string createdBy { get; set; }

    /// <summary>
    /// (Column: UPDATED_AT)
    /// </summary>
    public DateTime? updatedAt { get; set; }

    /// <summary>
    /// (Column: UPDATED_BY)
    /// </summary>
    public string updatedBy { get; set; }

    /// <summary>
    /// (Column: REMARKS)
    /// </summary>
    public string remarks { get; set; }

    /// <summary>
    /// (Child Table: CHILD_TABLE)
    /// </summary>
    public List<ChildTable> children { get; set; }
}

/// <summary>
/// (Table: CHILD_TABLE)
/// </summary>
public class ChildTable
{
    /// <summary>
    /// (Column: CHILD_ID)
    /// </summary>
    public int childId { get; set; }

    /// <summary>
    /// (Column: PARENT_ID)
    /// </summary>
    public int parentId { get; set; }

    /// <summary>
    /// (Column: CHILD_NAME)
    /// </summary>
    public string childName { get; set; }

    /// <summary>
    /// (Column: DESCRIPTION)
    /// </summary>
    public string description { get; set; }

    /// <summary>
    /// (Column: CREATED_AT)
    /// </summary>
    public DateTime createdAt { get; set; }

    /// <summary>
    /// (Column: STATUS)
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// (Column: IS_ACTIVE)
    /// </summary>
    public string isActive { get; set; }

    /// <summary>
    /// (Column: CREATED_BY)
    /// </summary>
    public string createdBy { get; set; }

    /// <summary>
    /// (Column: UPDATED_AT)
    /// </summary>
    public DateTime? updatedAt { get; set; }

    /// <summary>
    /// (Column: UPDATED_BY)
    /// </summary>
    public string updatedBy { get; set; }

    /// <summary>
    /// (Grandchild Table: GRANDCHILD_TABLE)
    /// </summary>
    public List<GrandchildTable> grandchildren { get; set; }
}

/// <summary>
/// (Table: GRANDCHILD_TABLE)
/// </summary>
public class GrandchildTable
{
    /// <summary>
    /// (Column: GRANDCHILD_ID)
    /// </summary>
    public int grandchildId { get; set; }

    /// <summary>
    /// (Column: CHILD_ID)
    /// </summary>
    public int childId { get; set; }

    /// <summary>
    /// (Column: DETAIL)
    /// </summary>
    public string detail { get; set; }

    /// <summary>
    /// (Column: CREATED_AT)
    /// </summary>
    public DateTime createdAt { get; set; }

    /// <summary>
    /// (Column: STATUS)
    /// </summary>
    public string status { get; set; }

    /// <summary>
    /// (Column: IS_ACTIVE)
    /// </summary>
    public string isActive { get; set; }

    /// <summary>
    /// (Column: CREATED_BY)
    /// </summary>
    public string createdBy { get; set; }

    /// <summary>
    /// (Column: UPDATED_AT)
    /// </summary>
    public DateTime? updatedAt { get; set; }

    /// <summary>
    /// (Column: UPDATED_BY)
    /// </summary>
    public string updatedBy { get; set; }
}";

        [BindProperty]
        public string SelectMethod { get; set; }

        [BindProperty]
        public string InsertsMethods { get; set; }

        [BindProperty]
        public string UpdateMethods { get; set; }

        [BindProperty]
        public string SelectRequest { get; set; }

        [BindProperty]
        [Required]
        public IFormFile SqlScriptFile { get; set; }

        [BindProperty]
        [Required]
        public IFormFile ModelClassesFile { get; set; }

        #endregion

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost()
        {
            string SqlScripts, ConnectedClasses;

            if(!ModelState.IsValid)
                return Page();


            using (var ms = new MemoryStream())
            {
                await SqlScriptFile.CopyToAsync(ms);

                var sqlArray = ms.ToArray();
                SqlScripts = Encoding.UTF8.GetString(sqlArray);

                ms.Position = 0;

                await ModelClassesFile.CopyToAsync(ms);
                var classArray = ms.ToArray();
                ConnectedClasses = Encoding.UTF8.GetString(classArray);
            }

            var builder = new DapperMethodBuilder(SqlScripts, ConnectedClasses);

            SelectMethod = builder.SqlSelectMethod;
            SelectRequest = builder.SqlSelectRequestSimple;
            ExampleTableSqlScripts = builder.TableScriptString;
            ExampleClassModels = builder.MappedClassesString;

            var insertMethods = new StringBuilder();
            var updateMethods = new StringBuilder();

            foreach (var mappedTable in builder.MappedTables)
            {
                insertMethods.AppendLine(mappedTable.InsertStringMethod);
                updateMethods.AppendLine(mappedTable.UpdateStringMethod);
            }

            InsertsMethods = insertMethods.ToString();
            UpdateMethods = updateMethods.ToString();

            return Page();
        }
    }
}
