using Evercore.Storage;

namespace StorageEngine.IntegrationTests;

public class MySqlStorageEngineTest : StorageEngineTestsBase, IClassFixture<MySqlContainerFixture>
{
    protected override IStorageEngine? StorageEngine { get; set; }
    
    public MySqlStorageEngineTest(MySqlContainerFixture containerFixture)
    {
        StorageEngine = containerFixture.StorageEngine;
    }
}