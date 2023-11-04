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
        InitializeZonePrices(app);

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

    private static void InitializeZonePrices(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var dbContext = scope.ServiceProvider.GetService<DatabaseContext>()!;

        foreach (var zone in Enum.GetValuesAsUnderlyingType<ParkingZone>())
        {
            var parkingZone = (ParkingZone)zone;
            var item = dbContext.ZonePrices.OrderByDescending(x => x.CreatedAtUtc)
                .LastOrDefault(x => x.Zone == parkingZone);

            if (item is null)
            {
                dbContext.Add(new ZonePrice
                {
                    Zone = parkingZone,
                    Price = parkingZone switch
                    {
                        ParkingZone.Zone1 => 2m,
                        ParkingZone.Zone2 => 1.5m,
                        ParkingZone.Zone3 => 1.0m,
                        ParkingZone.Zone4 => 0.5m,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                });
            }
        }

        dbContext.SaveChanges();
    }
}