namespace Evercore.Tools;

public class LruCache<TKey, TValue> where TKey: notnull
{
    #region Fields
    
    private readonly Dictionary<TKey, CachedItem<TKey,TValue>> nodeMap = new();
    private Func<TKey, CancellationToken, Task<TValue>> cacheMiss;
    private readonly LinkedList<CachedItem<TKey, TValue>> cacheList = new ();
    
    #endregion

    #region Properties


    public uint Capacity { get; }
    public int Count => cacheList.Count;
   
    public ulong Retrievals { get; private set; }
    public ulong Hits { get; private set; }
    public ulong Misses { get; private set; }
    public ulong Expired { get; private set; }
    public bool Full => cacheList.Count >= Capacity;
    public TimeSpan? TTL { get; }

    #endregion

    #region Constructors/Destructors
    
    public LruCache(uint capacity, Func<TKey,CancellationToken, Task<TValue>> cacheMiss, TimeSpan? ttl = null)
    {
        Capacity = capacity;
        this.cacheMiss = cacheMiss;
        TTL = ttl;
    }

    #endregion

    #region Methods

    public async Task<TValue> Get(TKey key, CancellationToken cancellationToken = default, DateTime? now = null)
    {
        now ??= DateTime.UtcNow;
        lock (this)
        {
            ++Retrievals;
            if (nodeMap.TryGetValue(key, out var found))
            {
                if (TTL is not null && (now - found.RetrievedAt) > TTL)
                {
                    ++Expired;
                    cacheList.Remove(found);
                    nodeMap.Remove(found.Key);
                }
                else
                {
                    ++Hits;
                    found.AccessCount++;
                    cacheList.Remove(found);
                    cacheList.AddFirst(found);
                    return found.Value;
                }
            }
            ++Misses;
        }
        
        var retrievedValue = await cacheMiss(key, cancellationToken);
        
        lock (this)
        {
            while (Full)
            {
                var last = cacheList.Last;
                if (last is not null && (now - last.Value.RetrievedAt > TTL))
                {
                    cacheList.RemoveLast();
                    nodeMap.Remove(last.Value.Key);
                }
                else
                {
                    break;
                }
            }

            if (Full)
            {
                var last = cacheList.Last;
                if (last is not null)
                {
                    cacheList.RemoveLast();
                    nodeMap.Remove(last.Value.Key);
                }
            }

            var newNode = new CachedItem<TKey, TValue>(key, retrievedValue, now.Value);
            cacheList.AddFirst(newNode);
            nodeMap.Add(key, newNode);
        }
        return retrievedValue;
    }

    public bool Contains(TKey key)
    {
        lock (this)
        {
            return nodeMap.ContainsKey(key);
        }
    }

    public IEnumerable<CachedItem<TKey, TValue>> Walk()
    {
        return cacheList;
    }
    
    #endregion
    
    #region Internal Types

    public class CachedItem<TUKey, TUValue>
    {

        public TUKey Key { get; }
        public TUValue Value { get; }
        public ulong AccessCount { get; set; } = 1;
        public DateTime RetrievedAt { get;  }
        
        public CachedItem(TUKey key, TUValue value, DateTime retrievedAt)
        {
            Key = key;
            Value = value;
            RetrievedAt = retrievedAt;
        }
    }

    #endregion

}