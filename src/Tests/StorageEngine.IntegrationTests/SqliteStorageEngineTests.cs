using Evercore.Storage;
using Evercore.Storage.SqlKata;
using Evercore.Storage.SqlKata.FluentMigrations;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;
using Testcontainers.PostgreSql;

namespace StorageEngine.IntegrationTests;

public class PostgresStorageEngineTest : StorageEngineTestsBase, IClassFixture<PostgresContainerFixture>
{
    protected override IStorageEngine? StorageEngine { get; set; }
    
    public PostgresStorageEngineTest(PostgresContainerFixture containerFixture)
    {
        StorageEngine = containerFixture.StorageEngine;
    }
}

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

public class SqliteStorageEngineTests : StorageEngineTestsBase, IAsyncLifetime
{
    protected override IStorageEngine? StorageEngine { get; set; }
    private string tempDbFile = "";

    public async Task InitializeAsync()
    {
        tempDbFile = Path.GetTempFileName();
        var connectionString = $"Data Source={tempDbFile}";
        var compiler = new SqliteCompiler();
        MigrationRunnerExecutor.MigrateUp((x) =>
        {
            x.AddSQLite().WithGlobalConnectionString(connectionString);
        });
        
        StorageEngine = new SqlKataStorageEngine(async (cancellationToken) =>
        {
            var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return await Task.FromResult(new QueryFactory(connection, compiler));
        });
        
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        File.Delete(tempDbFile);
        await Task.CompletedTask;
    }
}