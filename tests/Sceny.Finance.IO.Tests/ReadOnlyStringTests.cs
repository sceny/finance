using System;
using System.Collections.Generic;
using System.Globalization;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Tests;

public class ReadOnlyStringTests
{
    [Fact]
    public void Constructor_WithString_CreatesReadOnlyString()
    {
        // Arrange
        var value = "test";

        // Act
        var ros = new ReadOnlyString(value);

        // Assert
        Assert.Equal(value, ros.ToString());
        Assert.Equal(value.Length, ros.Length);
    }

    [Fact]
    public void Constructor_WithReadOnlyMemory_CreatesReadOnlyString()
    {
        // Arrange
        var memory = "test".AsMemory();

        // Act
        var ros = new ReadOnlyString(memory);

        // Assert
        Assert.Equal("test", ros.ToString());
    }

    [Fact]
    public void ImplicitConversion_FromString_Works()
    {
        // Act
        ReadOnlyString ros = "test";

        // Assert
        Assert.Equal("test", ros.ToString());
    }

    [Fact]
    public void ImplicitConversion_ToString_Works()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        string str = ros;

        // Assert
        Assert.Equal("test", str);
    }

    [Fact]
    public void ImplicitConversion_FromReadOnlyMemory_Works()
    {
        // Arrange
        var memory = "test".AsMemory();

        // Act
        ReadOnlyString ros = memory;

        // Assert
        Assert.Equal("test", ros.ToString());
    }

    [Fact]
    public void ImplicitConversion_ToReadOnlyMemory_Works()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        ReadOnlyMemory<char> memory = ros;

        // Assert
        Assert.Equal("test", memory.Span.ToString());
    }

    [Fact]
    public void ImplicitConversion_ToReadOnlySpan_Works()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        ReadOnlySpan<char> span = ros;

        // Assert
        Assert.Equal("test", span.ToString());
    }

    [Fact]
    public void IsEmpty_WithEmptyString_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros = string.Empty;

        // Assert
        Assert.True(ros.IsEmpty);
    }

    [Fact]
    public void IsEmpty_WithNonEmptyString_ReturnsFalse()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Assert
        Assert.False(ros.IsEmpty);
    }

    [Fact]
    public void Length_WithString_ReturnsCorrectLength()
    {
        // Arrange
        ReadOnlyString ros = "hello";

        // Assert
        Assert.Equal(5, ros.Length);
    }

    [Fact]
    public void Substring_WithStartIndex_ReturnsSubstring()
    {
        // Arrange
        ReadOnlyString ros = "hello world";

        // Act
        var result = ros.Substring(6);

        // Assert
        Assert.Equal("world", result.ToString());
    }

    [Fact]
    public void Substring_WithStartIndexAndLength_ReturnsSubstring()
    {
        // Arrange
        ReadOnlyString ros = "hello world";

        // Act
        var result = ros.Substring(0, 5);

        // Assert
        Assert.Equal("hello", result.ToString());
    }

    [Fact]
    public void Split_WithCharSeparator_ReturnsEnumerator()
    {
        // Arrange
        ReadOnlyString ros = "a,b,c";

        // Act
        var parts = new List<ReadOnlyString>();
        foreach (var part in ros.Split(','))
        {
            parts.Add(part);
        }

        // Assert
        Assert.Equal(3, parts.Count);
        Assert.Equal("a", parts[0].ToString());
        Assert.Equal("b", parts[1].ToString());
        Assert.Equal("c", parts[2].ToString());
    }

    [Fact]
    public void Split_WithCharSeparator_RemoveEmptyEntries_RemovesEmpty()
    {
        // Arrange
        ReadOnlyString ros = "a,,b,c";

        // Act
        var parts = new List<ReadOnlyString>();
        foreach (var part in ros.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            parts.Add(part);
        }

        // Assert
        Assert.Equal(3, parts.Count);
        Assert.Equal("a", parts[0].ToString());
        Assert.Equal("b", parts[1].ToString());
        Assert.Equal("c", parts[2].ToString());
    }

    [Fact]
    public void Contains_WithContainedValue_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros = "hello world";

        // Act
        var result = ros.Contains("world".AsSpan());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Contains_WithNotContainedValue_ReturnsFalse()
    {
        // Arrange
        ReadOnlyString ros = "hello world";

        // Act
        var result = ros.Contains("xyz".AsSpan());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void StartsWith_WithMatchingPrefix_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros = "hello world";

        // Act
        var result = ros.StartsWith("hello".AsSpan());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void StartsWith_WithNonMatchingPrefix_ReturnsFalse()
    {
        // Arrange
        ReadOnlyString ros = "hello world";

        // Act
        var result = ros.StartsWith("world".AsSpan());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void EndsWith_WithMatchingSuffix_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros = "hello world";

        // Act
        var result = ros.EndsWith("world".AsSpan());

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EndsWith_WithNonMatchingSuffix_ReturnsFalse()
    {
        // Arrange
        ReadOnlyString ros = "hello world";

        // Act
        var result = ros.EndsWith("hello".AsSpan());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IndexOf_WithContainedChar_ReturnsIndex()
    {
        // Arrange
        ReadOnlyString ros = "hello";

        // Act
        var result = ros.IndexOf('e');

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void IndexOf_WithNotContainedChar_ReturnsNegativeOne()
    {
        // Arrange
        ReadOnlyString ros = "hello";

        // Act
        var result = ros.IndexOf('x');

        // Assert
        Assert.Equal(-1, result);
    }

    [Fact]
    public void IndexOf_WithContainedSpan_ReturnsIndex()
    {
        // Arrange
        ReadOnlyString ros = "hello world";

        // Act
        var result = ros.IndexOf("world".AsSpan());

        // Assert
        Assert.Equal(6, result);
    }

    [Fact]
    public void Trim_WithWhitespace_RemovesWhitespace()
    {
        // Arrange
        ReadOnlyString ros = "  hello  ";

        // Act
        var result = ros.Trim();

        // Assert
        Assert.Equal("hello", result.ToString());
    }

    [Fact]
    public void Trim_WithNoWhitespace_ReturnsSame()
    {
        // Arrange
        ReadOnlyString ros = "hello";

        // Act
        var result = ros.Trim();

        // Assert
        Assert.Equal("hello", result.ToString());
    }

    [Fact]
    public void TrimStart_WithLeadingWhitespace_RemovesLeadingWhitespace()
    {
        // Arrange
        ReadOnlyString ros = "  hello";

        // Act
        var result = ros.TrimStart();

        // Assert
        Assert.Equal("hello", result.ToString());
    }

    [Fact]
    public void TrimEnd_WithTrailingWhitespace_RemovesTrailingWhitespace()
    {
        // Arrange
        ReadOnlyString ros = "hello  ";

        // Act
        var result = ros.TrimEnd();

        // Assert
        Assert.Equal("hello", result.ToString());
    }

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros1 = "test";
        ReadOnlyString ros2 = "test";

        // Act & Assert
        Assert.True(ros1.Equals(ros2));
        Assert.True(ros1 == ros2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        ReadOnlyString ros1 = "test";
        ReadOnlyString ros2 = "other";

        // Act & Assert
        Assert.False(ros1.Equals(ros2));
        Assert.True(ros1 != ros2);
    }

    [Fact]
    public void Equals_WithObject_ReturnsFalse()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act & Assert
        Assert.False(ros.Equals((object)"test"));
    }

    [Fact]
    public void GetHashCode_WithSameValue_ReturnsSameHashCode()
    {
        // Arrange
        ReadOnlyString ros1 = "test";
        ReadOnlyString ros2 = "test";

        // Act
        var hash1 = ros1.GetHashCode();
        var hash2 = ros2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void CompareTo_WithSmallerValue_ReturnsPositive()
    {
        // Arrange
        ReadOnlyString ros1 = "b";
        ReadOnlyString ros2 = "a";

        // Act
        var result = ros1.CompareTo(ros2);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void CompareTo_WithLargerValue_ReturnsNegative()
    {
        // Arrange
        ReadOnlyString ros1 = "a";
        ReadOnlyString ros2 = "b";

        // Act
        var result = ros1.CompareTo(ros2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void CompareTo_WithEqualValue_ReturnsZero()
    {
        // Arrange
        ReadOnlyString ros1 = "test";
        ReadOnlyString ros2 = "test";

        // Act
        var result = ros1.CompareTo(ros2);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void LessThanOperator_WithSmallerValue_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros1 = "a";
        ReadOnlyString ros2 = "b";

        // Act & Assert
        Assert.True(ros1 < ros2);
        Assert.True(ros1 <= ros2);
    }

    [Fact]
    public void GreaterThanOperator_WithLargerValue_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros1 = "b";
        ReadOnlyString ros2 = "a";

        // Act & Assert
        Assert.True(ros1 > ros2);
        Assert.True(ros1 >= ros2);
    }

    [Fact]
    public void Parse_WithString_ReturnsReadOnlyString()
    {
        // Act
        var result = ReadOnlyString.Parse("test", CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("test", result.ToString());
    }

    [Fact]
    public void TryParse_WithValidString_ReturnsTrue()
    {
        // Act
        var success = ReadOnlyString.TryParse("test", CultureInfo.InvariantCulture, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal("test", result.ToString());
    }

    [Fact]
    public void TryParse_WithNullString_ReturnsFalse()
    {
        // Act
        var success = ReadOnlyString.TryParse(null, CultureInfo.InvariantCulture, out var result);

        // Assert
        Assert.False(success);
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void Parse_WithSpan_ReturnsReadOnlyString()
    {
        // Arrange
        var span = "test".AsSpan();

        // Act
        var result = ReadOnlyString.Parse(span, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("test", result.ToString());
    }

    [Fact]
    public void TryParse_WithSpan_ReturnsTrue()
    {
        // Arrange
        var span = "test".AsSpan();

        // Act
        var success = ReadOnlyString.TryParse(span, CultureInfo.InvariantCulture, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal("test", result.ToString());
    }

    [Fact]
    public void TryParse_WithEmptySpan_ReturnsTrueWithDefault()
    {
        // Arrange
        var span = ReadOnlySpan<char>.Empty;

        // Act
        var success = ReadOnlyString.TryParse(span, CultureInfo.InvariantCulture, out var result);

        // Assert
        Assert.True(success);
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void Enumerate_IteratesThroughCharacters()
    {
        // Arrange
        ReadOnlyString ros = "abc";

        // Act
        var chars = new List<char>();
        foreach (var c in ros.Enumerate())
        {
            chars.Add(c);
        }

        // Assert
        Assert.Equal(3, chars.Count);
        Assert.Equal('a', chars[0]);
        Assert.Equal('b', chars[1]);
        Assert.Equal('c', chars[2]);
    }

    [Fact]
    public void Memory_ReturnsCorrectMemory()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        var memory = ros.Memory;

        // Assert
        Assert.Equal("test", memory.Span.ToString());
    }

    [Fact]
    public void Span_ReturnsCorrectSpan()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        var span = ros.Span;

        // Assert
        Assert.Equal("test", span.ToString());
    }

    [Fact]
    public void Substring_WithZeroLength_ReturnsEmpty()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        var result = ros.Substring(4);

        // Assert
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void Substring_WithFullLength_ReturnsSame()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        var result = ros.Substring(0, 4);

        // Assert
        Assert.Equal("test", result.ToString());
    }

    [Fact]
    public void Substring_WithStartIndexZero_ReturnsFull()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        var result = ros.Substring(0);

        // Assert
        Assert.Equal("test", result.ToString());
    }

    [Fact]
    public void Split_WithNoSeparator_ReturnsSingleElement()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        var parts = new List<ReadOnlyString>();
        foreach (var part in ros.Split(','))
        {
            parts.Add(part);
        }

        // Assert
        Assert.Single(parts);
        Assert.Equal("test", parts[0].ToString());
    }

    [Fact]
    public void Split_WithEmptyString_ReturnsEmptyEnumerator()
    {
        // Arrange
        ReadOnlyString ros = string.Empty;

        // Act
        var parts = new List<ReadOnlyString>();
        foreach (var part in ros.Split(','))
        {
            parts.Add(part);
        }

        // Assert
        Assert.Empty(parts);
    }

    [Fact]
    public void Split_WithOnlySeparator_ReturnsEmptyEnumerator()
    {
        // Arrange
        ReadOnlyString ros = ",";

        // Act
        var parts = new List<ReadOnlyString>();
        foreach (var part in ros.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            parts.Add(part);
        }

        // Assert
        Assert.Empty(parts);
    }

    [Fact]
    public void Split_WithOnlySeparator_KeepEmpty_ReturnsTwoEmpty()
    {
        // Arrange
        ReadOnlyString ros = ",";

        // Act
        var parts = new List<ReadOnlyString>();
        foreach (var part in ros.Split(','))
        {
            parts.Add(part);
        }

        // Assert
        Assert.Equal(2, parts.Count);
        Assert.True(parts[0].IsEmpty);
        Assert.True(parts[1].IsEmpty);
    }

    [Fact]
    public void Contains_WithEmptyValue_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        var result = ros.Contains(ReadOnlySpan<char>.Empty);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void StartsWith_WithEmptyValue_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        var result = ros.StartsWith(ReadOnlySpan<char>.Empty);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EndsWith_WithEmptyValue_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        var result = ros.EndsWith(ReadOnlySpan<char>.Empty);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IndexOf_WithEmptySpan_ReturnsZero()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act
        var result = ros.IndexOf(ReadOnlySpan<char>.Empty);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void Trim_WithOnlyWhitespace_ReturnsEmpty()
    {
        // Arrange
        ReadOnlyString ros = "   ";

        // Act
        var result = ros.Trim();

        // Assert
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void TrimStart_WithOnlyWhitespace_ReturnsEmpty()
    {
        // Arrange
        ReadOnlyString ros = "   ";

        // Act
        var result = ros.TrimStart();

        // Assert
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void TrimEnd_WithOnlyWhitespace_ReturnsEmpty()
    {
        // Arrange
        ReadOnlyString ros = "   ";

        // Act
        var result = ros.TrimEnd();

        // Assert
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void TrimEnd_WithNoTrailingWhitespace_ReturnsSame()
    {
        // Arrange
        ReadOnlyString ros = "hello";

        // Act
        var result = ros.TrimEnd();

        // Assert
        Assert.Equal("hello", result.ToString());
        Assert.Equal(ros, result);
    }

    [Fact]
    public void Equals_WithNullObject_ReturnsFalse()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act & Assert
        Assert.False(ros.Equals((object?)null));
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        ReadOnlyString ros = "test";

        // Act & Assert
        // Note: ReadOnlyString.Equals(object?) checks if obj is ReadOnlyString
        // Comparing with a string object will return false since it's not a ReadOnlyString
        Assert.False(ros.Equals((object)"test"));
    }

    [Fact]
    public void Equals_WithReadOnlyStringObject_ReturnsTrue()
    {
        // Arrange
        ReadOnlyString ros1 = "test";
        ReadOnlyString ros2 = "test";

        // Act & Assert
        // This tests the branch where obj IS a ReadOnlyString
        Assert.True(ros1.Equals((object)ros2));
    }

    [Fact]
    public void Equals_WithReadOnlyStringObjectDifferentValue_ReturnsFalse()
    {
        // Arrange
        ReadOnlyString ros1 = "test";
        ReadOnlyString ros2 = "other";

        // Act & Assert
        // This tests the branch where obj IS a ReadOnlyString but values differ
        Assert.False(ros1.Equals((object)ros2));
    }

    [Fact]
    public void GetHashCode_WithDifferentValue_ReturnsDifferentHashCode()
    {
        // Arrange
        ReadOnlyString ros1 = "test";
        ReadOnlyString ros2 = "other";

        // Act
        var hash1 = ros1.GetHashCode();
        var hash2 = ros2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void CompareTo_WithEmptyValue_ReturnsPositive()
    {
        // Arrange
        ReadOnlyString ros1 = "a";
        ReadOnlyString ros2 = string.Empty;

        // Act
        var result = ros1.CompareTo(ros2);

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void CompareTo_WithEmptySelf_ReturnsNegative()
    {
        // Arrange
        ReadOnlyString ros1 = string.Empty;
        ReadOnlyString ros2 = "a";

        // Act
        var result = ros1.CompareTo(ros2);

        // Assert
        Assert.True(result < 0);
    }

    [Fact]
    public void Parse_WithNullString_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ReadOnlyString.Parse(null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Parse_WithSpan_Allocates()
    {
        // Arrange
        var span = "test".AsSpan();

        // Act
        var result = ReadOnlyString.Parse(span, CultureInfo.InvariantCulture);

        // Assert
        Assert.Equal("test", result.ToString());
    }

    [Fact]
    public void Parse_WithEmptySpan_ReturnsDefault()
    {
        // Arrange
        var span = ReadOnlySpan<char>.Empty;

        // Act
        var result = ReadOnlyString.Parse(span, CultureInfo.InvariantCulture);

        // Assert
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void Enumerate_WithEmptyString_ReturnsNoItems()
    {
        // Arrange
        ReadOnlyString ros = string.Empty;

        // Act
        var count = 0;
        foreach (var _ in ros.Enumerate())
        {
            count++;
        }

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void Contains_WithStringComparison_Works()
    {
        // Arrange
        ReadOnlyString ros = "Test";

        // Act
        var resultOrdinal = ros.Contains("test".AsSpan(), StringComparison.Ordinal);
        var resultIgnoreCase = ros.Contains("test".AsSpan(), StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.False(resultOrdinal);
        Assert.True(resultIgnoreCase);
    }

    [Fact]
    public void StartsWith_WithStringComparison_Works()
    {
        // Arrange
        ReadOnlyString ros = "Test";

        // Act
        var resultOrdinal = ros.StartsWith("test".AsSpan(), StringComparison.Ordinal);
        var resultIgnoreCase = ros.StartsWith("test".AsSpan(), StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.False(resultOrdinal);
        Assert.True(resultIgnoreCase);
    }

    [Fact]
    public void EndsWith_WithStringComparison_Works()
    {
        // Arrange
        ReadOnlyString ros = "Test";

        // Act
        var resultOrdinal = ros.EndsWith("EST".AsSpan(), StringComparison.Ordinal);
        var resultIgnoreCase = ros.EndsWith("est".AsSpan(), StringComparison.OrdinalIgnoreCase);

        // Assert
        Assert.False(resultOrdinal);
        Assert.True(resultIgnoreCase);
    }
}

