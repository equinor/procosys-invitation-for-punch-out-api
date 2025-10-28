using Equinor.ProCoSys.IPO.WebApi.Middleware;
using Equinor.ProCoSys.IPO.WebApi.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Equinor.ProCoSys.IPO.WebApi.Extensions;

public static class ConfigureDatabaseExtension
{
    public static void ConfigureDatabase(this IHostApplicationBuilder builder)
    {
        MigrateDatabase(builder);
        SeedDummyData(builder);
    }

    private static void MigrateDatabase(IHostApplicationBuilder builder)
    {
        var environment = builder.Environment;

        if (!environment.IsDevelopment() && !environment.IsEnvironment("Test"))
        {
            return;
        }

        var migrateDatabase = builder.Configuration.GetValue<bool>("Application:MigrateDatabase");
        if (!migrateDatabase)
        {
            return;
        }

        builder.Services.AddHostedService<DatabaseMigrator>();
    }

    private static void SeedDummyData(IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment())
        {
            return;
        }

        if (!builder.Configuration.GetValue<bool>("Application:SeedDummyData"))
        {
            return;
        }

        builder.Services.AddHostedService<Seeder>();
    }
}
