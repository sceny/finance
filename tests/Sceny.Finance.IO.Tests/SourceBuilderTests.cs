using Sceny.Finance.IO;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO.Tests;

public class SourceBuilderTests
{
    [Fact]
    public void Source_WithValidSource_ReturnsSource()
    {
        // Arrange
        var builder = FinanceReader.FromString("test");

        // Act
        var source = builder.Source;

        // Assert
        Assert.NotNull(source);
    }

    [Fact]
    public void Source_WithFile_ReturnsFileSource()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test");
            var builder = FinanceReader.FromFile(tempFile);

            // Act
            var source = builder.Source;

            // Assert
            Assert.NotNull(source);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SourceBuilder<MemorySource>(null!));
    }
}

