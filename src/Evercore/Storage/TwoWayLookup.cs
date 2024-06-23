using System.Numerics;

namespace Evercore.Storage;

/// <summary>
/// Represents a two-way lookup dictionary that allows retrieving a value by key or a key by value.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
internal class TwoWayLookup<TKey, TValue> where TValue : INumber<TValue> where TKey : notnull
{
    private TValue counter = TValue.Zero;
    private Dictionary<TKey, TValue> lookup = new();
    private Dictionary<TValue, TKey> reversed = new();

    public TValue GetByKey(TKey key)
    {
        lock (this)
        {
            if (!lookup.TryGetValue(key, out TValue? value))
            {
                ++counter;
                value = counter;
                lookup[key] = value;
                reversed[value] = key;
            }
            return value;
        }
    }
    
    public TKey? GetByValue(TValue value)
    {
        lock (this)
        {
            return reversed.GetValueOrDefault(value);
        }
    }

    public TValue this[TKey index] => GetByKey(index);
    public TKey? this[TValue index] => GetByValue(index);
}