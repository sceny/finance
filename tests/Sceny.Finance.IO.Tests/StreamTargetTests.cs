using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO.Tests;

public class StreamTargetTests
{
    [Fact]
    public void Constructor_WithValidStream_SetsStream()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var target = new StreamTarget(stream);

        // Assert
        Assert.NotNull(target);
        Assert.Same(stream, target.Stream);
        Assert.False(target.DisposeStream);
    }

    [Fact]
    public void Constructor_WithNullStream_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StreamTarget(null!));
    }

    [Fact]
    public void Constructor_WithDisposeStream_SetsDisposeFlag()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var target = new StreamTarget(stream, disposeStream: true);

        // Assert
        Assert.NotNull(target);
        Assert.True(target.DisposeStream);
    }

    [Fact]
    public void Constructor_WithUnwritableStream_ThrowsArgumentException()
    {
        // Arrange
        var stream = new ReadOnlyStream();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new StreamTarget(stream));
    }

    [Fact]
    public void GetPipeWriter_WithValidStream_ReturnsPipeWriter()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var target = new StreamTarget(stream);
        var pipeWriter = target.GetPipeWriter();

        // Assert
        Assert.NotNull(pipeWriter);
        pipeWriter.Complete();
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithValidStream_ReturnsPipeWriter()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var target = new StreamTarget(stream);
        var pipeWriter = await target.GetPipeWriterAsync();

        // Assert
        Assert.NotNull(pipeWriter);
        await pipeWriter.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithDisposeStream_ReturnsPipeWriter()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var target = new StreamTarget(stream, disposeStream: true);
        var pipeWriter = await target.GetPipeWriterAsync();

        // Assert
        Assert.NotNull(pipeWriter);
        // Note: Stream is not disposed immediately, only when writer completes
        Assert.True(target.DisposeStream);
        await pipeWriter.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithCancellation_MayNotThrowImmediately()
    {
        // Arrange
        var stream = new MemoryStream();
        var target = new StreamTarget(stream);
        var cts = new CancellationTokenSource();
        
        // Act
        var writer = await target.GetPipeWriterAsync(cts.Token);
        
        // Cancel after writer is created - cancellation may not propagate immediately
        cts.Cancel();
        
        // Assert - Writer should still be valid
        Assert.NotNull(writer);
        await writer.CompleteAsync();
    }

    private sealed class ReadOnlyStream : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 0;
        public override long Position { get => 0; set { } }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => 0;
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}

