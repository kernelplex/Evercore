using Evercore.Storage.SqlKata;
using Evercore.Storage.SqlKata.FluentMigrations;
using FluentMigrator.Runner;
using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;
using Testcontainers.PostgreSql;

namespace StorageEngine.IntegrationTests;

public class PostgresContainerFixture : IDisposable
{
    private PostgreSqlContainer _dbContainer;

    public string ConnectionString => _dbContainer.GetConnectionString();
    public SqlKataStorageEngine StorageEngine { get; }

    public PostgresContainerFixture()
    {
        _dbContainer = BuildContainer();
        var compiler = new PostgresCompiler();
        var task = _dbContainer.StartAsync();
        task.Wait();
        var connectionString = _dbContainer.GetConnectionString();
        ApplyMigrations(connectionString);
        StorageEngine = new SqlKataStorageEngine(async (CancellationToken stoppingToken) =>
        {
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(stoppingToken);
            return new QueryFactory(connection, compiler);
        });
    }
    
    public static PostgreSqlContainer BuildContainer()
    {
        var dbcontainer = new PostgreSqlBuilder()
            .WithDatabase("app")
            .WithUsername("postgresql")
            .WithPassword("S3cr3t")
            .Build();
        var startTask = dbcontainer.StartAsync();
        startTask.Wait();
        return dbcontainer;
    }
    
    public static void ApplyMigrations(string connectionString)
    {
        MigrationRunnerExecutor.MigrateUp((x) =>
        {
            x.AddPostgres().WithGlobalConnectionString(connectionString);
        });
    }

    public void Dispose()
    {
        var stopTask = _dbContainer.StopAsync();
        stopTask.Wait();
        var disposeTask = _dbContainer.DisposeAsync();
        disposeTask.AsTask().Wait();
    }
}