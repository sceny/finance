using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO.Tests;

public class FileSourceTests
{
    [Fact]
    public void Constructor_WithValidPath_SetsFilePath()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        PipeReader? pipeReader = null;
        try
        {
            // Act
            var source = new FileSource(tempFile);

            // Assert
            Assert.NotNull(source);
            Assert.Equal(tempFile, source.FilePath);
            pipeReader = source.GetPipeReader();
            Assert.NotNull(pipeReader);
            pipeReader.Complete();
        }
        finally
        {
            pipeReader?.Complete();
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileSource(null!));
    }

    [Fact]
    public void Constructor_WithWhitespacePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new FileSource("   "));
    }

    [Fact]
    public void GetPipeReader_WithValidFile_ReturnsPipeReader()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test");

            // Act
            var source = new FileSource(tempFile);
            var pipeReader = source.GetPipeReader();

            // Assert
            Assert.NotNull(pipeReader);
            pipeReader.Complete();
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithValidFile_ReturnsPipeReader()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test");

            // Act
            var source = new FileSource(tempFile);
            var pipeReader = await source.GetPipeReaderAsync();

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
    public async Task GetPipeReaderAsync_WithCancellation_Throws()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test");
            var source = new FileSource(tempFile);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await source.GetPipeReaderAsync(cts.Token);
            });
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetPipeReader_WithNonExistentFile_Throws()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");

        // Act
        var source = new FileSource(nonExistentFile);

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => source.GetPipeReader());
    }
}

