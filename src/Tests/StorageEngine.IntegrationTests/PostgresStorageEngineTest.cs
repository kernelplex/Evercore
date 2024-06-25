using Evercore.Storage;

namespace StorageEngine.IntegrationTests;

public class PostgresStorageEngineTest : StorageEngineTestsBase, IClassFixture<PostgresContainerFixture>
{
    protected override IStorageEngine? StorageEngine { get; set; }
    
    public PostgresStorageEngineTest(PostgresContainerFixture containerFixture)
    {
        StorageEngine = containerFixture.StorageEngine;
    }
}