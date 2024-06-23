using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace Evercore.Storage.SqlKata.FluentMigrations;

public static class MigrationRunnerExecutor
{
    public static void MigrateUp(Action<IMigrationRunnerBuilder> buildMigration, long? version = null)
    {
        var runner = ServiceBuilder(buildMigration);
        if (version is null)
        {
            runner!.MigrateUp();
        }
        else
        {
            runner!.MigrateUp(version.Value);
        }
    }

    private static IMigrationRunner? ServiceBuilder(Action<IMigrationRunnerBuilder> buildMigration)
    {
        var services = new ServiceCollection();
        var assembly = typeof(MigrationsAnchor).Assembly;
        services.AddFluentMigratorCore()
            .ConfigureRunner((x) =>
            {
                buildMigration(x);
                x.ScanIn(assembly);
            });
        var provider = services.BuildServiceProvider();
        var runner = provider.GetService<IMigrationRunner>();
        return runner;
    }

    public static void MigrateDown(Action<IMigrationRunnerBuilder> buildMigrationRunner, long version) 
    {
        var runner = ServiceBuilder(buildMigrationRunner);
        runner!.MigrateDown(version);
    }
}