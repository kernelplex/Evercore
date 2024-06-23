using Evercore.Tools;
using FluentAssertions;

namespace Evercore.Tests;

public class LruCacheTests
{
    LruCache<string, int> _lruCache;
    private int counter;
    private int calledCount;
    private readonly uint capacity = 10;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(5);

    public LruCacheTests()
    {
        _lruCache = new LruCache<string, int>(capacity, async (_, _) =>
        {
            await Task.CompletedTask;
            calledCount++;
            counter++;
            return counter;
        }, ttl: _ttl);
    }

    [Fact]
    public async Task CanAdd()
    {
        var value = await _lruCache.Get("first");
        value.Should().Be(1);
        calledCount.Should().Be(1);
    }

    [Fact]
    public async Task RetrievesExistingFromCache()
    {
        var key = "single";
        var value = await _lruCache.Get(key);
        var value2 = await _lruCache.Get(key);
        value2.Should().Be(value);
        calledCount.Should().Be(1);
    }
    
    [Fact]
    public async Task CanAddMultiple()
    {
        const int itemsToAdd = 10;
        for (var j = 0; j < itemsToAdd; ++j)
        {
            _ = await _lruCache.Get($"entry{j}");
        }
        
        for (var j = 0; j < itemsToAdd; ++j)
        {
            _ = await _lruCache.Get($"entry{j}");
        }
        
        foreach (var node in _lruCache.Walk())
        {
            node.AccessCount.Should().Be(2);
        }
    }

    [Fact]
    public async Task OldestBumped()
    {
        var itemsToAdd = _lruCache.Capacity - 1;
        
        // This key should get bumped.
        var oldestKey = "oldest";
        _ = await _lruCache.Get(oldestKey);
       
        // Add up to the capacity, but don't go over.
        for (var j = 0; j < itemsToAdd; ++j)
        {
            _ = await _lruCache.Get($"entry{j}");
        }

        // Ensure the oldest is in there.
        var locateOldest = _lruCache.Walk()
            .Where(x => x.Key == oldestKey).ToList();
        locateOldest.Count.Should().Be(1);

        // This entry ought to bump the oldest.
        _ = _lruCache.Get("newest");

        _lruCache.Contains(oldestKey).Should().BeFalse();
        // Verify the oldest is no longer in there.
        _lruCache.Walk()
            .Any(x => x.Key == oldestKey).Should().BeFalse();
    }

    [Fact]
    public async Task EnsureExpires()
    {
        var initialTime = DateTime.UtcNow;
        var middleTime = DateTime.UtcNow + _ttl - TimeSpan.FromMilliseconds(1);
        var finalTime = DateTime.UtcNow + _ttl + TimeSpan.FromMinutes(1);
        var accessKey = "shouldExpire";
        _ = await _lruCache.Get(accessKey, now: initialTime);
        calledCount.Should().Be(1);
        
        _ = await _lruCache.Get(accessKey, now: middleTime);
        calledCount.Should().Be(1);
        
        _ = await _lruCache.Get(accessKey, now: finalTime);
        calledCount.Should().Be(2);
    }
}