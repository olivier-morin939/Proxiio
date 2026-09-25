using System.Text.Json;
using Entities.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;
using Services;

var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
var appSettingsPath = Path.Combine(repositoryRoot, "CRUDCoursesApp", "appsettings.json");
if (!File.Exists(appSettingsPath))
{
    Console.Error.WriteLine($"Configuration introuvable : {appSettingsPath}");
    return 1;
}

using var settings = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
var connectionString = settings.RootElement
    .GetProperty("ConnectionStrings")
    .GetProperty("DefaultConnection")
    .GetString();
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("La chaîne ConnectionStrings:DefaultConnection est vide.");
    return 1;
}

var services = new ServiceCollection();
services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(
    connectionString,
    sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));
services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

using var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
try
{
    await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>().Initialize();
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    var userCount = await db.Users.CountAsync();
    var communityCount = await db.Comunities.CountAsync();
    var postCount = await db.Posts.CountAsync();
    Console.WriteLine($"Migrations appliquées et données initiales vérifiées. Utilisateurs: {userCount}, communautés: {communityCount}, publications: {postCount}.");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Échec de l'initialisation EF : {exception.GetBaseException().Message}");
    return 1;
}
