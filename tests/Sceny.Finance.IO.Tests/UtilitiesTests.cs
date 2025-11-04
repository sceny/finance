using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO;

namespace Sceny.Finance.IO.Tests;

public class UtilitiesTests
{
    [Fact]
    public void ByteArrayPool_ReturnsSharedPool()
    {
        // Act
        var pool1 = Utilities.ByteArrayPool;
        var pool2 = Utilities.ByteArrayPool;

        // Assert
        Assert.Same(pool1, pool2);
    }

    [Fact]
    public void CharArrayPool_ReturnsSharedPool()
    {
        // Act
        var pool1 = Utilities.CharArrayPool;
        var pool2 = Utilities.CharArrayPool;

        // Assert
        Assert.Same(pool1, pool2);
    }

    [Fact]
    public async Task ReadCompleteSequenceAsync_WithSingleSegment_ReturnsSequence()
    {
        // Arrange
        var pipe = new Pipe();
        var writer = pipe.Writer;
        var data = Encoding.UTF8.GetBytes("test");
        var span = writer.GetSpan(data.Length);
        data.CopyTo(span);
        writer.Advance(data.Length);
        await writer.FlushAsync();
        writer.Complete();

        // Act
        var sequence = await Utilities.ReadCompleteSequenceAsync(pipe.Reader);

        // Assert
        Assert.Equal(data.Length, sequence.Length);
        var result = Encoding.UTF8.GetString(sequence.FirstSpan);
        Assert.Equal("test", result);
        
        pipe.Reader.Complete();
    }

    [Fact]
    public async Task ReadCompleteSequenceAsync_WithMultipleSegments_ReturnsCompleteSequence()
    {
        // Arrange
        var pipe = new Pipe();
        var writer = pipe.Writer;
        var data1 = Encoding.UTF8.GetBytes("hello ");
        var data2 = Encoding.UTF8.GetBytes("world");
        var span1 = writer.GetSpan(data1.Length);
        data1.CopyTo(span1);
        writer.Advance(data1.Length);
        await writer.FlushAsync();
        var span2 = writer.GetSpan(data2.Length);
        data2.CopyTo(span2);
        writer.Advance(data2.Length);
        await writer.FlushAsync();
        writer.Complete();

        // Act
        var sequence = await Utilities.ReadCompleteSequenceAsync(pipe.Reader);
        var result = Utilities.SequenceToString(sequence, Encoding.UTF8);

        // Assert
        Assert.Equal(data1.Length + data2.Length, sequence.Length);
        Assert.Equal("hello world", result);
        
        pipe.Reader.Complete();
    }

    [Fact]
    public async Task ReadCompleteSequenceAsync_WithCancellation_Cancels()
    {
        // Arrange
        var pipe = new Pipe();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await Utilities.ReadCompleteSequenceAsync(pipe.Reader, cts.Token);
        });
        
        pipe.Reader.Complete();
        pipe.Writer.Complete();
    }

    [Fact]
    public void SequenceToString_WithSingleSegment_ReturnsString()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test");
        var sequence = new ReadOnlySequence<byte>(data);

        // Act
        var result = Utilities.SequenceToString(sequence, Encoding.UTF8);

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public void SequenceToString_WithEmptySequence_ReturnsEmptyString()
    {
        // Arrange
        var sequence = ReadOnlySequence<byte>.Empty;

        // Act
        var result = Utilities.SequenceToString(sequence, Encoding.UTF8);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SequenceToString_WithDifferentEncoding_HandlesEncoding()
    {
        // Arrange
        var data = Encoding.Unicode.GetBytes("test");
        var sequence = new ReadOnlySequence<byte>(data);

        // Act
        var result = Utilities.SequenceToString(sequence, Encoding.Unicode);

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public async Task AdvanceReader_AdvancesPipeReader()
    {
        // Arrange
        var pipe = new Pipe();
        var writer = pipe.Writer;
        var data = Encoding.UTF8.GetBytes("test");
        var span = writer.GetSpan(data.Length);
        data.CopyTo(span);
        writer.Advance(data.Length);
        writer.Complete();

        // Act
        var sequence = await Utilities.ReadCompleteSequenceAsync(pipe.Reader);
        Utilities.AdvanceReader(pipe.Reader, sequence);

        // Assert - should not throw
        pipe.Reader.Complete();
    }

    [Fact]
    public void StringToMemory_ReturnsMemory()
    {
        // Arrange
        var value = "test";

        // Act
        var memory = Utilities.StringToMemory(value);

        // Assert
        Assert.Equal(value, memory.Span.ToString());
    }
}

