using System.Linq;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO.Tests;

public class CsvSourceBuilderExtensionsTests
{
    [Fact]
    public void AsCsv_WithValidBuilder_ReturnsConfiguredReader()
    {
        // Arrange
        var builder = FinanceReader.FromString("AccountId,AccountName\nACC001,Test");

        // Act
        var reader = builder.AsCsv();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void AsCsv_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CsvSourceBuilderExtensions.AsCsv<StringSource>(null!));
    }

    [Fact]
    public void AsCsv_WithConfiguration_ReturnsConfiguredReader()
    {
        // Arrange
        var builder = FinanceReader.FromString("AccountId,AccountName\nACC001,Test");

        // Act
        var reader = builder.AsCsv(options => options.Delimiter = ';');

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void AsCsv_WithNullBuilderAndConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            CsvSourceBuilderExtensions.AsCsv<StringSource>(null!, options => options.Delimiter = ';'));
    }

    [Fact]
    public void AsCsv_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = FinanceReader.FromString("AccountId,AccountName\nACC001,Test");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AsCsv(null!));
    }

    [Fact]
    public async Task AsCsv_WithValidData_CanReadAccounts()
    {
        // Arrange
        var builder = FinanceReader.FromString("AccountId,AccountName\nACC001,Test");

        // Act
        var reader = builder.AsCsv();
        var accounts = await reader.GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task AsCsv_WithConfiguration_AppliesConfiguration()
    {
        // Arrange
        var builder = FinanceReader.FromString("AccountId;AccountName\nACC001;Test");

        // Act
        var reader = builder.AsCsv(options => options.Delimiter = ';');
        var accounts = await reader.GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }
}

