using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO.Tests;

public class StreamSourceTests
{
    [Fact]
    public void Constructor_WithValidStream_SetsStream()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var source = new StreamSource(stream);

        // Assert
        Assert.NotNull(source);
        Assert.Same(stream, source.Stream);
        var pipeReader = source.GetPipeReader();
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void Constructor_WithNullStream_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StreamSource(null!));
    }

    [Fact]
    public void Constructor_WithDisposeStream_SetsDisposeFlag()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var source = new StreamSource(stream, disposeStream: true);

        // Assert
        Assert.NotNull(source);
        Assert.True(source.DisposeStream);
        var pipeReader = source.GetPipeReader();
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void GetPipeReader_WithValidStream_ReturnsPipeReader()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var source = new StreamSource(stream);
        var pipeReader = source.GetPipeReader();

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithValidStream_ReturnsPipeReader()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var source = new StreamSource(stream);
        var pipeReader = await source.GetPipeReaderAsync();

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithDisposeStream_DisposesStream()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var source = new StreamSource(stream, disposeStream: true);
        var pipeReader = await source.GetPipeReaderAsync();

        // Assert
        Assert.NotNull(pipeReader);
        // Stream should be disposed after reader is created
        Assert.Throws<ObjectDisposedException>(() => stream.Position = 0);
        pipeReader.Complete();
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithoutDisposeStream_DoesNotDisposeStream()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var source = new StreamSource(stream, disposeStream: false);
        var pipeReader = await source.GetPipeReaderAsync();

        // Assert
        Assert.NotNull(pipeReader);
        // Stream should still be accessible
        stream.Position = 0;
        Assert.Equal(4, stream.Length);
        pipeReader.Complete();
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithCancellation_Throws()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var source = new StreamSource(stream);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await source.GetPipeReaderAsync(cts.Token);
        });
    }

    [Fact]
    public void Constructor_WithUnreadableStream_Throws()
    {
        using var stream = new WriteOnlyStream();
        Assert.Throws<ArgumentException>(() => new StreamSource(stream));
    }

    private sealed class WriteOnlyStream : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get => 0; set { } }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) { }
    }
}

