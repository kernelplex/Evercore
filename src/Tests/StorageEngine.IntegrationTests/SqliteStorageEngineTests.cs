using Evercore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace StorageEngine.IntegrationTests;

public class SqliteStorageEngineTests : StorageEngineTestsBase, IClassFixture<SqliteFixture> 
{
    protected override IStorageEngine? StorageEngine { get; set; }
    
    public SqliteStorageEngineTests(SqliteFixture containerFixture)
    {
        StorageEngine = containerFixture.StorageEngine;
    }

}