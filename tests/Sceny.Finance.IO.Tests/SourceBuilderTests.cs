using Sceny.Finance.IO;

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
}

