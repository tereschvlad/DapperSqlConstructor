namespace DapperSqlConstructor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            var app = builder.Build();

            app.UseExceptionHandler("/Error");
            app.UseHsts();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
