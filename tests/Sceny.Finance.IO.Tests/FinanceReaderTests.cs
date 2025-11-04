using System.Linq;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;
using Sceny.Finance.IO.Sources;

namespace Sceny.Finance.IO.Tests;

public class FinanceReaderTests
{
    [Fact]
    public void FromFile_WithValidPath_ReturnsSourceBuilder()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "AccountId,AccountName\nACC001,Test");

            // Act
            var builder = FinanceReader.FromFile(tempFile);

            // Assert
            Assert.NotNull(builder);
            Assert.NotNull(builder.Source);
            Assert.IsType<FileSource>(builder.Source);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void FromFile_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FinanceReader.FromFile(null!));
    }

    [Fact]
    public void FromStream_WithValidStream_ReturnsSourceBuilder()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var builder = FinanceReader.FromStream(stream);

        // Assert
        Assert.NotNull(builder);
        Assert.NotNull(builder.Source);
        Assert.IsType<StreamSource>(builder.Source);
    }

    [Fact]
    public void FromStream_WithNullStream_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FinanceReader.FromStream(null!));
    }

    [Fact]
    public void FromStream_WithDisposeStream_ReturnsSourceBuilder()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test"));

        // Act
        var builder = FinanceReader.FromStream(stream, disposeStream: true);

        // Assert
        Assert.NotNull(builder);
        Assert.NotNull(builder.Source);
        Assert.IsType<StreamSource>(builder.Source);
    }

    [Fact]
    public void FromBytes_WithValidData_ReturnsSourceBuilder()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test").AsMemory();

        // Act
        var builder = FinanceReader.FromBytes(data);

        // Assert
        Assert.NotNull(builder);
        Assert.NotNull(builder.Source);
        Assert.IsType<MemorySource>(builder.Source);
    }

    [Fact]
    public void FromString_WithValidString_ReturnsSourceBuilder()
    {
        // Arrange
        var content = "test";

        // Act
        var builder = FinanceReader.FromString(content);

        // Assert
        Assert.NotNull(builder);
        Assert.NotNull(builder.Source);
        Assert.IsType<StringSource>(builder.Source);
    }

    [Fact]
    public void FromString_WithNullString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => FinanceReader.FromString(null!));
    }

    [Fact]
    public void FromString_WithCustomEncoding_ReturnsSourceBuilder()
    {
        // Arrange
        var content = "test";
        var encoding = Encoding.UTF32;

        // Act
        var builder = FinanceReader.FromString(content, encoding);

        // Assert
        Assert.NotNull(builder);
        Assert.NotNull(builder.Source);
        Assert.IsType<StringSource>(builder.Source);
    }

    [Fact]
    public async Task FromFile_WithCsv_CanReadAccounts()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "AccountId,AccountName,Type\nACC001,Test,Checking");

            // Act
            var accounts = await FinanceReader.FromFile(tempFile).AsCsv().GetAccountsAsync().ToListAsync();

            // Assert
            Assert.Single(accounts);
            Assert.Equal("ACC001", accounts[0].Id);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task FromStream_WithCsv_CanReadAccounts()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("AccountId,AccountName\nACC001,Test"));

        // Act
        var accounts = await FinanceReader.FromStream(stream).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task FromBytes_WithCsv_CanReadAccounts()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("AccountId,AccountName\nACC001,Test").AsMemory();

        // Act
        var accounts = await FinanceReader.FromBytes(data).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }

    [Fact]
    public async Task FromString_WithCsv_CanReadAccounts()
    {
        // Arrange
        var content = "AccountId,AccountName\nACC001,Test";

        // Act
        var accounts = await FinanceReader.FromString(content).AsCsv().GetAccountsAsync().ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("ACC001", accounts[0].Id);
    }
}

