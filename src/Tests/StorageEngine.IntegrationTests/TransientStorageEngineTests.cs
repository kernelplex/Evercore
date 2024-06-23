using Evercore.Storage;

namespace StorageEngine.IntegrationTests;

public class TransientStorageEngineTests: StorageEngineTestsBase
{
    protected override IStorageEngine? StorageEngine { get; set; }
    
    public TransientStorageEngineTests()
    {
        StorageEngine = new TransientMemoryStorageEngine();
    }
    
}