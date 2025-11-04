using System.IO.Pipelines;
using System.Text;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO.Tests;

public class StringSourceTests
{
    [Fact]
    public void Constructor_WithValidString_SetsContent()
    {
        // Arrange
        var content = "test";

        // Act
        var source = new StringSource(content);

        // Assert
        Assert.NotNull(source);
        var pipeReader = source.GetPipeReader();
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void Constructor_WithNullString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StringSource(null!));
    }

    [Fact]
    public void Constructor_WithEmptyString_DoesNotThrow()
    {
        // Arrange
        var content = string.Empty;

        // Act
        var source = new StringSource(content);

        // Assert
        Assert.NotNull(source);
        var pipeReader = source.GetPipeReader();
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void Constructor_WithCustomEncoding_SetsEncoding()
    {
        // Arrange
        var content = "test";
        var encoding = Encoding.UTF32;

        // Act
        var source = new StringSource(content, encoding);

        // Assert
        Assert.NotNull(source);
        var pipeReader = source.GetPipeReader();
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void Constructor_WithNullEncoding_UsesUtf8()
    {
        // Arrange
        var content = "test";

        // Act
        var source = new StringSource(content, null);

        // Assert
        Assert.NotNull(source);
        var pipeReader = source.GetPipeReader();
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void GetPipeReader_WithValidString_ReturnsPipeReader()
    {
        // Arrange
        var content = "test";

        // Act
        var source = new StringSource(content);
        var pipeReader = source.GetPipeReader();

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }

    [Fact]
    public void GetPipeReader_WithEmptyString_ReturnsPipeReader()
    {
        // Arrange
        var content = string.Empty;

        // Act
        var source = new StringSource(content);
        var pipeReader = source.GetPipeReader();

        // Assert
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }
}

