using Evercore.Storage.SqlKata;
using Evercore.Storage.SqlKata.FluentMigrations;
using FluentMigrator.Runner;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;
using Testcontainers.MySql;

namespace StorageEngine.IntegrationTests;

public class MySqlContainerFixture : IDisposable
{
    private MySqlContainer _dbContainer;

    public string ConnectionString => _dbContainer.GetConnectionString();
    public SqlKataStorageEngine StorageEngine { get; }

    public MySqlContainerFixture()
    {
        
        _dbContainer = BuildContainer();
        var compiler = new MySqlCompiler();
        var task = _dbContainer.StartAsync();
        task.Wait();
        var connectionString = _dbContainer.GetConnectionString();
        ApplyMigrations(connectionString);
        StorageEngine = new SqlKataStorageEngine(async stoppingToken =>
        {
            var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync(stoppingToken);
            return new QueryFactory(connection, compiler);
        });
    }
    
    public static MySqlContainer BuildContainer()
    {
        var dbcontainer = new MySqlBuilder()
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
            x.AddMySql8().WithGlobalConnectionString(connectionString);
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