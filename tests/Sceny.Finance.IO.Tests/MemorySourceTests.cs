using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO.Tests;

public class MemorySourceTests
{
    [Fact]
    public void Constructor_WithValidData_SetsData()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test").AsMemory();

        // Act
        var source = new MemorySource(data);

        // Assert
        Assert.NotNull(source);
        Assert.True(source.Data.Span.SequenceEqual(data.Span));
        var pipeReader = source.GetPipeReader();
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void Constructor_WithEmptyData_DoesNotThrow()
    {
        // Arrange
        var data = ReadOnlyMemory<byte>.Empty;

        // Act
        var source = new MemorySource(data);

        // Assert
        Assert.NotNull(source);
        Assert.True(source.Data.IsEmpty);
        var pipeReader = source.GetPipeReader();
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void GetPipeReader_WithValidData_ReturnsPipeReader()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test").AsMemory();

        // Act
        var source = new MemorySource(data);
        var pipeReader = source.GetPipeReader();

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void GetPipeReader_WithEmptyData_ReturnsPipeReader()
    {
        // Arrange
        var data = ReadOnlyMemory<byte>.Empty;

        // Act
        var source = new MemorySource(data);
        var pipeReader = source.GetPipeReader();

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }
}

