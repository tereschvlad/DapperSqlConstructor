using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using DapperSqlConstructor.Models;

namespace DapperSqlConstructor.Pages
{
    public class SqlConstructModel : PageModel
    {

        [BindProperty]
        [Required]
        public IFormFile SqlScriptFile { get; set; }

        [BindProperty]
        [Required]
        public IFormFile ModelClassesFile { get; set; }


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
            builder.ParseTableProperties();

            return Page();
        }
    }
}
