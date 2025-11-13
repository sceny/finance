using System.Buffers;
using System.Globalization;

namespace Sceny.Finance.IO;

/// <summary>
/// A zero-allocation wrapper for string data that can be used in readonly struct fields.
/// Provides implicit conversions to/from string, ReadOnlyMemory&lt;char&gt;, and ReadOnlySpan&lt;char&gt;.
/// Implements common string operations and interfaces for seamless usage.
/// </summary>
public readonly struct ReadOnlyString : 
    IComparable<ReadOnlyString>, 
    IEquatable<ReadOnlyString>,
    IParsable<ReadOnlyString>,
    ISpanParsable<ReadOnlyString>
{
    private readonly ReadOnlyMemory<char> _memory;

    /// <summary>
    /// Creates a ReadOnlyString from ReadOnlyMemory&lt;char&gt;.
    /// </summary>
    public ReadOnlyString(ReadOnlyMemory<char> memory)
    {
        _memory = memory;
    }

    /// <summary>
    /// Creates a ReadOnlyString from a string.
    /// </summary>
    public ReadOnlyString(string value)
    {
        _memory = value.AsMemory();
    }

    /// <summary>
    /// Gets the underlying ReadOnlyMemory&lt;char&gt;.
    /// </summary>
    public ReadOnlyMemory<char> Memory => _memory;

    /// <summary>
    /// Gets the underlying ReadOnlySpan&lt;char&gt; (zero-allocation).
    /// </summary>
    public ReadOnlySpan<char> Span => _memory.Span;

    /// <summary>
    /// Gets whether the string is empty.
    /// </summary>
    public bool IsEmpty => _memory.IsEmpty;

    /// <summary>
    /// Gets the length of the string.
    /// </summary>
    public int Length => _memory.Length;

    /// <summary>
    /// Implicit conversion from string to ReadOnlyString.
    /// </summary>
    public static implicit operator ReadOnlyString(string value)
    {
        return new ReadOnlyString(value);
    }

    /// <summary>
    /// Implicit conversion from ReadOnlyString to string (allocates).
    /// </summary>
    public static implicit operator string(ReadOnlyString value)
    {
        return value._memory.Span.ToString();
    }

    /// <summary>
    /// Implicit conversion from ReadOnlyMemory&lt;char&gt; to ReadOnlyString.
    /// </summary>
    public static implicit operator ReadOnlyString(ReadOnlyMemory<char> memory)
    {
        return new ReadOnlyString(memory);
    }

    /// <summary>
    /// Implicit conversion from ReadOnlyString to ReadOnlyMemory&lt;char&gt;.
    /// </summary>
    public static implicit operator ReadOnlyMemory<char>(ReadOnlyString value)
    {
        return value._memory;
    }

    /// <summary>
    /// Implicit conversion from ReadOnlyString to ReadOnlySpan&lt;char&gt; (zero-allocation).
    /// </summary>
    public static implicit operator ReadOnlySpan<char>(ReadOnlyString value)
    {
        return value._memory.Span;
    }

    /// <summary>
    /// Returns the string representation (allocates).
    /// </summary>
    public override string ToString()
    {
        return _memory.Span.ToString();
    }

    /// <summary>
    /// Returns a substring starting at the specified index (zero-allocation).
    /// </summary>
    public ReadOnlyString Substring(int startIndex)
    {
        return new ReadOnlyString(_memory.Slice(startIndex));
    }

    /// <summary>
    /// Returns a substring starting at the specified index with the specified length (zero-allocation).
    /// </summary>
    public ReadOnlyString Substring(int startIndex, int length)
    {
        return new ReadOnlyString(_memory.Slice(startIndex, length));
    }

    /// <summary>
    /// Returns a zero-allocation enumerator for splitting by a single character.
    /// </summary>
    public SplitEnumerator Split(char separator, StringSplitOptions options = StringSplitOptions.None)
    {
        return new SplitEnumerator(_memory, separator, options);
    }

    /// <summary>
    /// Zero-allocation enumerator for splitting ReadOnlyString by a delimiter.
    /// </summary>
    public ref struct SplitEnumerator
    {
        private readonly ReadOnlyMemory<char> _memory;
        private readonly char _separator;
        private readonly StringSplitOptions _options;
        private int _nextStart;
        private int _segmentStart;
        private int _segmentEnd;
        private bool _afterSeparator;

        internal SplitEnumerator(ReadOnlyMemory<char> memory, char separator, StringSplitOptions options)
        {
            _memory = memory;
            _separator = separator;
            _options = options;
            _nextStart = 0;
            _segmentStart = -1;
            _segmentEnd = -1;
            _afterSeparator = false;
        }

        public ReadOnlyString Current
        {
            get
            {
                if (_segmentEnd == -1)
                    throw new InvalidOperationException("Enumerator not started");
                
                var length = _segmentEnd - _segmentStart;
                if (length == 0 && _segmentStart >= _memory.Length)
                {
                    // Empty segment at the end - return empty ReadOnlyString
                    return default;
                }
                return new ReadOnlyString(_memory.Slice(_segmentStart, length));
            }
        }

        public bool MoveNext()
        {
            if (_memory.IsEmpty)
                return false;

            var span = _memory.Span;

            // If we're after a separator and at the end, yield empty segment if keeping empty entries
            if (_afterSeparator && _nextStart >= span.Length)
            {
                if (_options != StringSplitOptions.RemoveEmptyEntries)
                {
                    // Create empty segment at the end (slice from end with length 0)
                    _segmentStart = span.Length;
                    _segmentEnd = span.Length;
                    _nextStart = span.Length + 1; // Mark as done
                    _afterSeparator = false;
                    return true;
                }
                _nextStart = span.Length + 1; // Mark as done
                _afterSeparator = false;
                return false;
            }

            if (_nextStart > span.Length)
                return false;

            // Find next separator or end
            var separatorIndex = -1;
            for (int i = _nextStart; i < span.Length; i++)
            {
                if (span[i] == _separator)
                {
                    separatorIndex = i;
                    break;
                }
            }

            if (separatorIndex == -1)
            {
                // No more separators - yield final segment if we have content
                if (_nextStart < span.Length)
                {
                    _segmentStart = _nextStart;
                    _segmentEnd = span.Length;
                    _nextStart = span.Length + 1; // Mark as done
                    _afterSeparator = false;
                    return true;
                }
                _nextStart = span.Length + 1; // Mark as done
                _afterSeparator = false;
                return false;
            }

            // Found separator at separatorIndex
            // Yield segment from _nextStart to separatorIndex (before the separator)
            var hasSegmentContent = _nextStart < separatorIndex;
            
            if (!hasSegmentContent && _options == StringSplitOptions.RemoveEmptyEntries)
            {
                // Skip empty segment, move to next
                _nextStart = separatorIndex + 1;
                // Check if separator is at the end - if so, we need to yield empty segment after it
                if (_nextStart >= span.Length && _options != StringSplitOptions.RemoveEmptyEntries)
                {
                    _afterSeparator = true;
                    return MoveNext(); // This will handle the empty segment after separator
                }
                return MoveNext();
            }

            // Yield this segment (before the separator)
            _segmentStart = _nextStart;
            _segmentEnd = separatorIndex;
            _nextStart = separatorIndex + 1;
            
            // Check if separator is at the end - if so, mark that we need to yield empty segment after it
            if (_nextStart >= span.Length)
            {
                _afterSeparator = true;
            }
            else
            {
                _afterSeparator = false;
            }
            
            return true;
        }

        public void Reset()
        {
            _nextStart = 0;
            _segmentStart = -1;
            _segmentEnd = -1;
            _afterSeparator = false;
        }

        public SplitEnumerator GetEnumerator() => this;
    }

    /// <summary>
    /// Determines whether this string contains the specified value.
    /// </summary>
    public bool Contains(ReadOnlySpan<char> value, StringComparison comparisonType = StringComparison.Ordinal)
    {
        return _memory.Span.Contains(value, comparisonType);
    }

    /// <summary>
    /// Determines whether this string starts with the specified value.
    /// </summary>
    public bool StartsWith(ReadOnlySpan<char> value, StringComparison comparisonType = StringComparison.Ordinal)
    {
        return _memory.Span.StartsWith(value, comparisonType);
    }

    /// <summary>
    /// Determines whether this string ends with the specified value.
    /// </summary>
    public bool EndsWith(ReadOnlySpan<char> value, StringComparison comparisonType = StringComparison.Ordinal)
    {
        return _memory.Span.EndsWith(value, comparisonType);
    }

    /// <summary>
    /// Returns the index of the first occurrence of the specified value.
    /// </summary>
    public int IndexOf(char value)
    {
        return _memory.Span.IndexOf(value);
    }

    /// <summary>
    /// Returns the index of the first occurrence of the specified value.
    /// </summary>
    public int IndexOf(ReadOnlySpan<char> value)
    {
        return _memory.Span.IndexOf(value);
    }

    /// <summary>
    /// Returns a new ReadOnlyString with leading and trailing whitespace removed (zero-allocation).
    /// </summary>
    public ReadOnlyString Trim()
    {
        var span = _memory.Span;
        var trimmed = span.Trim();
        
        if (trimmed.Length == span.Length)
            return this;

        // Count leading whitespace to find start offset
        int startOffset = 0;
        while (startOffset < span.Length && char.IsWhiteSpace(span[startOffset]))
            startOffset++;

        return new ReadOnlyString(_memory.Slice(startOffset, trimmed.Length));
    }

    /// <summary>
    /// Returns a new ReadOnlyString with leading whitespace removed (zero-allocation).
    /// </summary>
    public ReadOnlyString TrimStart()
    {
        var span = _memory.Span;
        var trimmed = span.TrimStart();
        
        if (trimmed.Length == span.Length)
            return this;

        // Count leading whitespace to find start offset
        int startOffset = 0;
        while (startOffset < span.Length && char.IsWhiteSpace(span[startOffset]))
            startOffset++;

        return new ReadOnlyString(_memory.Slice(startOffset));
    }

    /// <summary>
    /// Returns a new ReadOnlyString with trailing whitespace removed (zero-allocation).
    /// </summary>
    public ReadOnlyString TrimEnd()
    {
        var span = _memory.Span;
        var trimmed = span.TrimEnd();
        
        if (trimmed.Length == span.Length)
            return this;

        // TrimEnd doesn't change start, so we can use the original start with trimmed length
        return new ReadOnlyString(_memory.Slice(0, trimmed.Length));
    }

    /// <summary>
    /// Compares this instance with another ReadOnlyString.
    /// </summary>
    public int CompareTo(ReadOnlyString other)
    {
        return _memory.Span.CompareTo(other._memory.Span, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether this instance equals another ReadOnlyString.
    /// </summary>
    public bool Equals(ReadOnlyString other)
    {
        return _memory.Span.SequenceEqual(other._memory.Span);
    }

    /// <summary>
    /// Determines whether this instance equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is ReadOnlyString other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
        return string.GetHashCode(_memory.Span, StringComparison.Ordinal);
    }

    /// <summary>
    /// Parses a ReadOnlyString from a string.
    /// </summary>
    public static ReadOnlyString Parse(string s, IFormatProvider? provider)
    {
        if (s == null)
            throw new ArgumentNullException(nameof(s));
        return new ReadOnlyString(s);
    }

    /// <summary>
    /// Attempts to parse a ReadOnlyString from a string.
    /// </summary>
    public static bool TryParse(string? s, IFormatProvider? provider, out ReadOnlyString result)
    {
        if (s == null)
        {
            result = default;
            return false;
        }

        result = new ReadOnlyString(s);
        return true;
    }

    /// <summary>
    /// Parses a ReadOnlyString from a ReadOnlySpan&lt;char&gt;.
    /// Uses ArrayPool to minimize allocations.
    /// </summary>
    public static ReadOnlyString Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (s.IsEmpty)
            return default;

        var pool = ArrayPool<char>.Shared;
        var array = pool.Rent(s.Length);
        try
        {
            s.CopyTo(array);
            return new ReadOnlyString(new ReadOnlyMemory<char>(array, 0, s.Length));
        }
        finally
        {
            pool.Return(array);
        }
    }

    /// <summary>
    /// Attempts to parse a ReadOnlyString from a ReadOnlySpan&lt;char&gt;.
    /// Uses ArrayPool to minimize allocations.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ReadOnlyString result)
    {
        if (s.IsEmpty)
        {
            result = default;
            return true;
        }

        var pool = ArrayPool<char>.Shared;
        var array = pool.Rent(s.Length);
        try
        {
            s.CopyTo(array);
            result = new ReadOnlyString(new ReadOnlyMemory<char>(array, 0, s.Length));
            return true;
        }
        finally
        {
            pool.Return(array);
        }
    }


    /// <summary>
    /// Returns a zero-allocation enumerator for iterating through characters.
    /// </summary>
    public CharEnumerator Enumerate()
    {
        return new CharEnumerator(_memory);
    }

    /// <summary>
    /// Zero-allocation enumerator for iterating through characters.
    /// </summary>
    public ref struct CharEnumerator
    {
        private readonly ReadOnlyMemory<char> _memory;
        private int _index;

        internal CharEnumerator(ReadOnlyMemory<char> memory)
        {
            _memory = memory;
            _index = -1;
        }

        public char Current => _memory.Span[_index];

        public bool MoveNext()
        {
            _index++;
            return _index < _memory.Length;
        }

        public void Reset()
        {
            _index = -1;
        }

        public CharEnumerator GetEnumerator() => this;
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(ReadOnlyString left, ReadOnlyString right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(ReadOnlyString left, ReadOnlyString right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Less-than operator.
    /// </summary>
    public static bool operator <(ReadOnlyString left, ReadOnlyString right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Greater-than operator.
    /// </summary>
    public static bool operator >(ReadOnlyString left, ReadOnlyString right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    /// Less-than-or-equal operator.
    /// </summary>
    public static bool operator <=(ReadOnlyString left, ReadOnlyString right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    /// Greater-than-or-equal operator.
    /// </summary>
    public static bool operator >=(ReadOnlyString left, ReadOnlyString right)
    {
        return left.CompareTo(right) >= 0;
    }
}
