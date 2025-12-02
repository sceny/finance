using System.Collections.Generic;
using System.Linq;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Tests;

public class ReadOnlyExtendedTests
{
    [Fact]
    public void Constructor_WithPairs_CreatesReadOnlyExtended()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1"),
            new("key2", "value2")
        };

        // Act
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Assert
        Assert.Equal(2, extended.Count);
        Assert.False(extended.IsEmpty);
    }

    [Fact]
    public void Constructor_WithEmptyPairs_CreatesEmptyExtended()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[0];

        // Act
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Assert
        Assert.Equal(0, extended.Count);
        Assert.True(extended.IsEmpty);
    }

    [Fact]
    public void TryGetValue_WithExistingKey_ReturnsTrueAndValue()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1"),
            new("key2", "value2")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Act
        var result = extended.TryGetValue("key1", out var value);

        // Assert
        Assert.True(result);
        Assert.Equal("value1", value.ToString());
    }

    [Fact]
    public void TryGetValue_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Act
        var result = extended.TryGetValue("nonexistent", out var value);

        // Assert
        Assert.False(result);
        Assert.True(value.IsEmpty);
    }

    [Fact]
    public void TryGetValue_WithEmptyExtended_ReturnsFalse()
    {
        // Arrange
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(new KeyValuePair<ReadOnlyString, ReadOnlyString>[0]));

        // Act
        var result = extended.TryGetValue("key1", out var value);

        // Assert
        Assert.False(result);
        Assert.True(value.IsEmpty);
    }

    [Fact]
    public void ContainsKey_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Act
        var result = extended.ContainsKey("key1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ContainsKey_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Act
        var result = extended.ContainsKey("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetValueOrDefault_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Act
        var value = extended.GetValueOrDefault("key1");

        // Assert
        Assert.Equal("value1", value.ToString());
    }

    [Fact]
    public void GetValueOrDefault_WithNonExistingKey_ReturnsDefault()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Act
        var value = extended.GetValueOrDefault("nonexistent");

        // Assert
        Assert.True(value.IsEmpty);
    }

    [Fact]
    public void GetEnumerator_EnumeratesAllPairs()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1"),
            new("key2", "value2"),
            new("key3", "value3")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Act
        var enumerated = extended.ToList();

        // Assert
        Assert.Equal(3, enumerated.Count);
        Assert.Equal("key1", enumerated[0].Key.ToString());
        Assert.Equal("value1", enumerated[0].Value.ToString());
        Assert.Equal("key2", enumerated[1].Key.ToString());
        Assert.Equal("value2", enumerated[1].Value.ToString());
        Assert.Equal("key3", enumerated[2].Key.ToString());
        Assert.Equal("value3", enumerated[2].Value.ToString());
    }

    [Fact]
    public void GetEnumerator_WithEmptyExtended_EnumeratesNothing()
    {
        // Arrange
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(new KeyValuePair<ReadOnlyString, ReadOnlyString>[0]));

        // Act
        var enumerated = extended.ToList();

        // Assert
        Assert.Empty(enumerated);
    }

    [Fact]
    public void Enumerator_Reset_ResetsToBeginning()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1"),
            new("key2", "value2")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));
        var enumerator = extended.GetEnumerator();

        // Act
        enumerator.MoveNext();
        var first = enumerator.Current;
        enumerator.Reset();
        enumerator.MoveNext();
        var afterReset = enumerator.Current;

        // Assert
        Assert.Equal(first.Key, afterReset.Key);
        Assert.Equal(first.Value, afterReset.Value);
    }

    [Fact]
    public void Enumerator_Dispose_DoesNotThrow()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));
        var enumerator = extended.GetEnumerator();

        // Act & Assert
        enumerator.Dispose(); // Should not throw
    }

    [Fact]
    public void IEnumerable_GetEnumerator_ReturnsEnumerator()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Act
        var enumerator = ((IEnumerable<KeyValuePair<ReadOnlyString, ReadOnlyString>>)extended).GetEnumerator();

        // Assert
        Assert.NotNull(enumerator);
        Assert.True(enumerator.MoveNext());
        Assert.Equal("key1", enumerator.Current.Key.ToString());
    }

    [Fact]
    public void IEnumerable_NonGeneric_GetEnumerator_ReturnsEnumerator()
    {
        // Arrange
        var pairs = new KeyValuePair<ReadOnlyString, ReadOnlyString>[]
        {
            new("key1", "value1")
        };
        var extended = new ReadOnlyExtended(new ReadOnlyMemory<KeyValuePair<ReadOnlyString, ReadOnlyString>>(pairs));

        // Act
        var enumerator = ((System.Collections.IEnumerable)extended).GetEnumerator();

        // Assert
        Assert.NotNull(enumerator);
        Assert.True(enumerator.MoveNext());
        var pair = (KeyValuePair<ReadOnlyString, ReadOnlyString>)enumerator.Current;
        Assert.Equal("key1", pair.Key.ToString());
    }
}

