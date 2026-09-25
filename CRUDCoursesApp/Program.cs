using Entities.Contexts;
using Services;
using ServiceContracts;
using Microsoft.EntityFrameworkCore;
namespace CRUDCoursesApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllersWithViews();
            builder.Services.AddTransient<IEncryptionsService, EncryptionsService>();
            builder.Services.AddScoped<IUsersService, UsersService>();
            builder.Services.AddScoped<IComunitiesService, ComunitiesService>();
            builder.Services.AddScoped<IPostsService, PostsService>();
            builder.Services.AddScoped<IComunityMembersService, ComunityMembersService>();
            builder.Services.AddScoped<IReportsService, ReportsService>();
            builder.Services.AddScoped<ISignalsService, SignaslService>();

            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure())
            );

            var app = builder.Build();
            app.UseRouting();
            app.UseStaticFiles();
            app.MapControllers();
            app.Run();
        }
    }
}
