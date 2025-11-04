using System.Collections;
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
    ISpanParsable<ReadOnlyString>,
    IEnumerable<char>
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
    /// Splits the string by the specified delimiter.
    /// </summary>
    public ReadOnlyString[] Split(char separator, StringSplitOptions options = StringSplitOptions.None)
    {
        if (_memory.IsEmpty)
            return Array.Empty<ReadOnlyString>();

        var span = _memory.Span;
        var parts = new List<ReadOnlyString>();
        int start = 0;

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] == separator)
            {
                if (start < i || options != StringSplitOptions.RemoveEmptyEntries)
                {
                    parts.Add(new ReadOnlyString(_memory.Slice(start, i - start)));
                }
                start = i + 1;
            }
        }

        if (start < span.Length || options != StringSplitOptions.RemoveEmptyEntries)
        {
            parts.Add(new ReadOnlyString(_memory.Slice(start)));
        }

        return parts.ToArray();
    }

    /// <summary>
    /// Splits the string by the specified string separator.
    /// </summary>
    public ReadOnlyString[] Split(string separator, StringSplitOptions options = StringSplitOptions.None)
    {
        var span = _memory.Span;
        var parts = new List<ReadOnlyString>();
        var separatorSpan = separator.AsSpan();
        int start = 0;

        while (start < span.Length)
        {
            var index = span.Slice(start).IndexOf(separatorSpan);
            if (index == -1)
            {
                parts.Add(new ReadOnlyString(_memory.Slice(start)));
                break;
            }

            var actualIndex = start + index;
            if (start < actualIndex || options != StringSplitOptions.RemoveEmptyEntries)
            {
                parts.Add(new ReadOnlyString(_memory.Slice(start, actualIndex - start)));
            }
            start = actualIndex + separatorSpan.Length;
        }

        return parts.ToArray();
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
    /// </summary>
    public static ReadOnlyString Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return new ReadOnlyString(s.ToArray().AsMemory());
    }

    /// <summary>
    /// Attempts to parse a ReadOnlyString from a ReadOnlySpan&lt;char&gt;.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ReadOnlyString result)
    {
        result = new ReadOnlyString(s.ToArray().AsMemory());
        return true;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the characters.
    /// </summary>
    public IEnumerator<char> GetEnumerator()
    {
        var memory = _memory;
        var length = memory.Length;
        for (int i = 0; i < length; i++)
        {
            yield return memory.Span[i];
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the characters.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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
