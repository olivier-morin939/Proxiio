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
            builder.Services.AddSingleton<IUsersService, UsersService>();
            builder.Services.AddSingleton<IComunitiesService, ComunitiesService>();
            var app = builder.Build();
            app.UseRouting();
            app.UseStaticFiles();
            app.MapControllers();
            app.Run();
        }
    }
}
