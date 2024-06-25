using Evercore.Storage.SqlKata;
using Evercore.Storage.SqlKata.FluentMigrations;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace StorageEngine.IntegrationTests;

public class SqliteFixture : IDisposable
{
    // public string ConnectionString => _dbContainer.GetConnectionString();
    public SqlKataStorageEngine StorageEngine { get; }
    readonly string tempDbFile;

    public SqliteFixture()
    {
        tempDbFile = Path.GetTempFileName();
        var connectionString = $"Data Source={tempDbFile}";
        MigrationRunnerExecutor.MigrateUp((x) =>
        {
            x.AddSQLite().WithGlobalConnectionString(connectionString);
        });
        
        var compiler = new SqliteCompiler();
        
        StorageEngine = new SqlKataStorageEngine(async (cancellationToken) =>
        {
            var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            return await Task.FromResult(new QueryFactory(connection, compiler));
        });
    }
    

    public void Dispose()
    {
        File.Delete(tempDbFile);
    }
}