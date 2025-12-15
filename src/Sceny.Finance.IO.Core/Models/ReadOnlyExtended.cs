using System.Collections;

namespace Sceny.Finance.IO;

/// <summary>
/// Zero-allocation key-value store for unstructured properties (Extended).
/// Provides dictionary-like access to unmapped source values.
/// </summary>
public readonly struct ReadOnlyExtended : IEnumerable<KeyValuePair<ReadOnlyString, ReadOnlyString>>
{
    private readonly ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>> _pairs;

    /// <summary>
    /// Creates a ReadOnlyExtended from key-value pairs.
    /// </summary>
    public ReadOnlyExtended(ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>> pairs)
    {
        _pairs = pairs;
    }

    /// <summary>
    /// Gets the number of key-value pairs.
    /// </summary>
    public int Count => _pairs.Length;

    /// <summary>
    /// Gets whether the Extended collection is empty.
    /// </summary>
    public bool IsEmpty => _pairs.IsEmpty;

    /// <summary>
    /// Attempts to get the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to look up</param>
    /// <param name="value">When this method returns, contains the value associated with the key, if found</param>
    /// <returns>True if the key was found; otherwise, false</returns>
    public bool TryGetValue(ReadOnlyString key, out ReadOnlyString value)
    {
        var span = _pairs.Span;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].Key.Equals(key))
            {
                value = span[i].Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Determines whether the Extended collection contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate</param>
    /// <returns>True if the key is found; otherwise, false</returns>
    public bool ContainsKey(ReadOnlyString key)
    {
        return TryGetValue(key, out _);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key to look up</param>
    /// <returns>The value if found; otherwise, default ReadOnlyString</returns>
    public ReadOnlyString GetValueOrDefault(ReadOnlyString key)
    {
        TryGetValue(key, out var value);
        return value;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the key-value pairs.
    /// </summary>
    public Enumerator GetEnumerator() => new Enumerator(_pairs);

    IEnumerator<KeyValuePair<ReadOnlyString, ReadOnlyString>> IEnumerable<KeyValuePair<ReadOnlyString, ReadOnlyString>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Zero-allocation enumerator for ReadOnlyExtended key-value pairs.
    /// </summary>
    public struct Enumerator : IEnumerator<KeyValuePair<ReadOnlyString, ReadOnlyString>>
    {
        private readonly ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>> _pairs;
        private int _index;

        internal Enumerator(ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>> pairs)
        {
            _pairs = pairs;
            _index = -1;
        }

        public KeyValuePair<ReadOnlyString, ReadOnlyString> Current => _pairs.Span[_index];

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;
            return _index < _pairs.Length;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
        }
    }
}




















