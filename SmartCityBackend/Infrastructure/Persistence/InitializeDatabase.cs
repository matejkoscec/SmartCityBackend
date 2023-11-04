using FluentMigrator.Runner;
using SmartCityBackend.Models;

namespace SmartCityBackend.Infrastructure.Persistence;

public static class InitializeDatabase
{
    public static IApplicationBuilder InitializeDb(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var migrator = scope.ServiceProvider.GetService<IMigrationRunner>()!;
        migrator.MigrateUp();

        InitializeRoles(app);

        return app;
    }

    private static void InitializeRoles(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var context = scope.ServiceProvider.GetService<DatabaseContext>()!;

        var configuration = scope.ServiceProvider.GetService<IConfiguration>()!;
        var configRoles = configuration.GetSection("Auth:Roles").Get<List<string>>();

        if (configRoles is null || !configRoles.Any())
        {
            throw new Exception("Roles not found in configuration");
        }

        var existingRoles = context.Roles.ToList();
        foreach (var configRole in configRoles.Where(configRole => existingRoles.All(r => r.Name != configRole)))
        {
            context.Roles.Add(new Role { Name = configRole });
        }

        context.SaveChanges();
    }
}