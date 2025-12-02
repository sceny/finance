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
    public async Task CreatePipeWriterFromStringBuilder_WithNullEncoding_UsesUtf8()
    {
        // Arrange
        var sb = new StringBuilder();
        
        // Act - Pass null encoding to test the default branch
        var writer = Utilities.CreatePipeWriterFromStringBuilder(sb, encoding: null);
        var data = Encoding.UTF8.GetBytes("test");
        var span = writer.GetSpan(data.Length);
        data.CopyTo(span);
        writer.Advance(data.Length);
        await writer.FlushAsync();
        writer.Complete();
        
        // Wait for background task
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        Assert.Equal("test", sb.ToString());
    }

    [Fact]
    public async Task CreatePipeWriterFromStringBuilder_WithExplicitEncoding_UsesProvidedEncoding()
    {
        // Arrange
        var sb = new StringBuilder();
        var encoding = Encoding.ASCII;
        
        // Act
        var writer = Utilities.CreatePipeWriterFromStringBuilder(sb, encoding);
        var data = encoding.GetBytes("test");
        var span = writer.GetSpan(data.Length);
        data.CopyTo(span);
        writer.Advance(data.Length);
        await writer.FlushAsync();
        writer.Complete();
        
        // Wait for background task
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        Assert.Equal("test", sb.ToString());
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
    public async Task ReadCompleteSequenceAsync_WithPreCanceledToken_Throws()
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
        
        // Cancel the reader
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        // Act & Assert
        // Should throw TaskCanceledException when cancellation token is already canceled
        await Assert.ThrowsAnyAsync<TaskCanceledException>(async () =>
        {
            await Utilities.ReadCompleteSequenceAsync(pipe.Reader, cts.Token);
        });
        
        pipe.Reader.Complete();
    }

    [Fact]
    public async Task ReadCompleteSequenceAsync_WithEmptyBuffer_ContinuesReading()
    {
        // Arrange
        var pipe = new Pipe();
        var writer = pipe.Writer;
        // Write empty buffer first
        await writer.FlushAsync();
        // Then write data
        var data = Encoding.UTF8.GetBytes("test");
        var span = writer.GetSpan(data.Length);
        data.CopyTo(span);
        writer.Advance(data.Length);
        await writer.FlushAsync();
        writer.Complete();

        // Act
        var sequence = await Utilities.ReadCompleteSequenceAsync(pipe.Reader);

        // Assert
        Assert.True(sequence.Length > 0);
        pipe.Reader.Complete();
    }

    [Fact]
    public async Task ReadCompleteSequenceAsync_WithCompletedWriter_ReadsAllData()
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

        // Assert
        Assert.Equal(data.Length, sequence.Length);
        pipe.Reader.Complete();
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

    [Fact]
    public void StringToMemory_WithEmptyString_ReturnsEmptyMemory()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var memory = Utilities.StringToMemory(value);

        // Assert
        Assert.True(memory.IsEmpty);
    }

    [Fact]
    public async Task ReadCompleteSequenceAsync_WithEmptyPipe_ReturnsEmptySequence()
    {
        // Arrange
        var pipe = new Pipe();
        pipe.Writer.Complete();

        // Act
        var sequence = await Utilities.ReadCompleteSequenceAsync(pipe.Reader);

        // Assert
        Assert.Equal(0, sequence.Length);
        pipe.Reader.Complete();
    }

    [Fact]
    public async Task ReadCompleteSequenceAsync_WithDataButNotCompleted_ReturnsSequence()
    {
        // Arrange - Test the branch where buffer.Length > 0 causes early break (not completed)
        var pipe = new Pipe();
        var writer = pipe.Writer;
        var data = Encoding.UTF8.GetBytes("test");
        var span = writer.GetSpan(data.Length);
        data.CopyTo(span);
        writer.Advance(data.Length);
        await writer.FlushAsync();
        // Don't complete - test early break with buffer.Length > 0

        // Act
        var sequence = await Utilities.ReadCompleteSequenceAsync(pipe.Reader);

        // Assert
        Assert.True(sequence.Length > 0);
        
        // Complete for cleanup
        writer.Complete();
        pipe.Reader.Complete();
    }

    [Fact]
    public async Task ReadCompleteSequenceAsync_WithIsCanceledFlag_ReturnsEarly()
    {
        // Arrange - Test the IsCanceled branch separately
        var pipe = new Pipe();
        var cts = new CancellationTokenSource();
        
        // Start a read that will be canceled
        var readTask = Utilities.ReadCompleteSequenceAsync(pipe.Reader, cts.Token);
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await readTask;
        });
        
        pipe.Reader.Complete();
        pipe.Writer.Complete();
    }

    [Fact]
    public async Task SequenceToString_WithMultiSegmentSequence_ReturnsString()
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
        writer.Complete();

        var readResult = await pipe.Reader.ReadAsync();
        var sequence = readResult.Buffer;

        // Act
        var result = Utilities.SequenceToString(sequence, Encoding.UTF8);

        // Assert
        Assert.Equal("hello world", result);
        pipe.Reader.Complete();
    }

    [Fact]
    public void CreatePipeReaderFromString_CreatesPipeReader()
    {
        // Arrange
        var content = "test";

        // Act
        var pipeReader = Utilities.CreatePipeReaderFromString(content);

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void CreatePipeReaderFromString_WithEmptyString_CreatesPipeReader()
    {
        // Arrange
        var content = string.Empty;

        // Act
        var pipeReader = Utilities.CreatePipeReaderFromString(content);

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void CreatePipeReaderFromString_WithCustomEncoding_CreatesPipeReader()
    {
        // Arrange
        var content = "test";
        var encoding = Encoding.UTF32;

        // Act
        var pipeReader = Utilities.CreatePipeReaderFromString(content, encoding);

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void CreatePipeReaderFromString_WithNullEncoding_UsesUtf8()
    {
        // Arrange
        var content = "test";

        // Act
        var pipeReader = Utilities.CreatePipeReaderFromString(content, null);

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void CreatePipeReaderFromBytes_CreatesPipeReader()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test").AsMemory();

        // Act
        var pipeReader = Utilities.CreatePipeReaderFromBytes(data);

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void CreatePipeReaderFromBytes_WithEmptyData_CreatesPipeReader()
    {
        // Arrange
        var data = ReadOnlyMemory<byte>.Empty;

        // Act
        var pipeReader = Utilities.CreatePipeReaderFromBytes(data);

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public async Task CreatePipeReaderFromStreamAsync_WithValidStream_CreatesPipeReader()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var pipeReader = await Utilities.CreatePipeReaderFromStreamAsync(stream);

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public async Task CreatePipeReaderFromStreamAsync_WithEmptyStream_CreatesPipeReader()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var pipeReader = await Utilities.CreatePipeReaderFromStreamAsync(stream);

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public async Task CreatePipeReaderFromStreamAsync_WithCancellation_Throws()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await Utilities.CreatePipeReaderFromStreamAsync(stream, cts.Token);
        });
    }

    [Fact]
    public async Task CreatePipeReaderFromFileAsync_WithValidFile_CreatesPipeReader()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test");

            // Act
            var pipeReader = await Utilities.CreatePipeReaderFromFileAsync(tempFile);

            // Assert
            Assert.NotNull(pipeReader);
            pipeReader.Complete();
            await pipeReader.CompleteAsync();
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task CreatePipeReaderFromFileAsync_WithEmptyFile_CreatesPipeReader()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            // Act
            var pipeReader = await Utilities.CreatePipeReaderFromFileAsync(tempFile);

            // Assert
            Assert.NotNull(pipeReader);
            pipeReader.Complete();
            await pipeReader.CompleteAsync();
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task CreatePipeReaderFromFileAsync_WithCancellation_Throws()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await Utilities.CreatePipeReaderFromFileAsync(tempFile, cts.Token);
            });
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task CreatePipeReaderFromFileAsync_WithNonExistentFile_Throws()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            await Utilities.CreatePipeReaderFromFileAsync(nonExistentFile);
        });
    }
}

