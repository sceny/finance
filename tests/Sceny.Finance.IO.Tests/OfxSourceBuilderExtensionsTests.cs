using System.Linq;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Ofx;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO.Tests;

public class OfxSourceBuilderExtensionsTests
{
    [Fact]
    public void AsOfx_WithValidBuilder_ReturnsConfiguredReader()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM><ACCTID>ACC001</ACCTID></BANKACCTFROM>";
        var builder = FinanceReader.FromString(ofx);

        // Act
        var reader = builder.AsOfx();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void AsOfx_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => OfxSourceBuilderExtensions.AsOfx<StringSource>(null!));
    }

    [Fact]
    public void AsOfx_WithConfiguration_ReturnsConfiguredReader()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM><ACCTID>ACC001</ACCTID></BANKACCTFROM>";
        var builder = FinanceReader.FromString(ofx);

        // Act
        var reader = builder.AsOfx(options => options.DateFormat = "yyyyMMdd");

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void AsOfx_WithNullBuilderAndConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            OfxSourceBuilderExtensions.AsOfx<StringSource>(null!, options => options.DateFormat = "yyyyMMdd"));
    }

    [Fact]
    public void AsOfx_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM><ACCTID>ACC001</ACCTID></BANKACCTFROM>";
        var builder = FinanceReader.FromString(ofx);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AsOfx(null!));
    }

    [Fact]
    public async Task AsOfx_WithValidData_CanReadAccounts()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM><ACCTID>ACC001</ACCTID></BANKACCTFROM>";
        var builder = FinanceReader.FromString(ofx);

        // Act
        var reader = builder.AsOfx();
        var accounts = await reader.GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task AsOfx_WithConfiguration_AppliesConfiguration()
    {
        // Arrange
        var ofx = @"<BANKACCTFROM><ACCTID>ACC001</ACCTID></BANKACCTFROM>";
        var builder = FinanceReader.FromString(ofx);

        // Act
        var reader = builder.AsOfx(options => options.DateFormat = "yyyyMMdd");
        var accounts = await reader.GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }
}

