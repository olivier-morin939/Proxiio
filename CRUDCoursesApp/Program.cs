using Services;
using ServiceContracts;
namespace CRUDCoursesApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<ICoursesService, CoursesService>();
            var app = builder.Build();
            app.UseRouting();
            app.UseStaticFiles();
            app.MapControllers();
            app.Run();
        }
    }
}
