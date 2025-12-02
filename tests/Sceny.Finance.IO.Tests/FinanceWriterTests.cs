using System.Buffers;
using System.IO;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO.Tests;

public class FinanceWriterTests
{
    [Fact]
    public void ToFile_WithValidPath_ReturnsTargetBuilder()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            // Act
            var builder = FinanceWriter.ToFile(tempFile);

            // Assert
            Assert.NotNull(builder);
            Assert.NotNull(builder.Target);
            Assert.IsType<FileTarget>(builder.Target);
            Assert.Equal(tempFile, builder.Target.FilePath);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ToFile_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FinanceWriter.ToFile(null!));
    }

    [Fact]
    public void ToStream_WithValidStream_ReturnsTargetBuilder()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var builder = FinanceWriter.ToStream(stream);

        // Assert
        Assert.NotNull(builder);
        Assert.NotNull(builder.Target);
        Assert.IsType<StreamTarget>(builder.Target);
    }

    [Fact]
    public void ToStream_WithNullStream_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FinanceWriter.ToStream(null!));
    }

    [Fact]
    public void ToStream_WithDisposeStream_ReturnsTargetBuilder()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var builder = FinanceWriter.ToStream(stream, disposeStream: true);

        // Assert
        Assert.NotNull(builder);
        Assert.NotNull(builder.Target);
        Assert.IsType<StreamTarget>(builder.Target);
    }

    [Fact]
    public void ToBytes_WithValidBufferWriter_ReturnsTargetBuilder()
    {
        // Arrange
        var bufferWriter = new ArrayBufferWriter<byte>();

        // Act
        var builder = FinanceWriter.ToBytes(bufferWriter);

        // Assert
        Assert.NotNull(builder);
        Assert.NotNull(builder.Target);
        Assert.IsType<MemoryTarget>(builder.Target);
        Assert.Equal(bufferWriter, builder.Target.BufferWriter);
    }

    [Fact]
    public void ToBytes_WithNullBufferWriter_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FinanceWriter.ToBytes(null!));
    }

    [Fact]
    public void ToString_WithValidStringBuilder_ReturnsTargetBuilder()
    {
        // Arrange
        var sb = new StringBuilder();

        // Act
        var builder = FinanceWriter.ToString(sb);

        // Assert
        Assert.NotNull(builder);
        Assert.NotNull(builder.Target);
        Assert.IsType<StringTarget>(builder.Target);
        Assert.Equal(sb, builder.Target.StringBuilder);
    }

    [Fact]
    public void ToString_WithNullStringBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FinanceWriter.ToString(null!));
    }

    [Fact]
    public void ToString_WithCustomEncoding_UsesEncoding()
    {
        // Arrange
        var sb = new StringBuilder();
        var encoding = Encoding.UTF32;

        // Act
        var builder = FinanceWriter.ToString(sb, encoding);

        // Assert
        Assert.NotNull(builder);
        Assert.Equal(encoding, builder.Target.Encoding);
    }
}

