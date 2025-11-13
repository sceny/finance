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
        Assert.Equal(content, source.Content);
        Assert.Equal(Encoding.UTF8, source.Encoding);
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
    public void Constructor_WithEmptyString_AllowsEmptyContent()
    {
        var source = new StringSource(string.Empty);
        Assert.NotNull(source);
        Assert.Equal(string.Empty, source.Content);
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
        Assert.Equal(encoding, source.Encoding);
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
        Assert.Equal(Encoding.UTF8, source.Encoding);
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
        var source = new StringSource(string.Empty);
        var pipeReader = source.GetPipeReader();
        Assert.NotNull(pipeReader);
        pipeReader.Complete();
    }
}

