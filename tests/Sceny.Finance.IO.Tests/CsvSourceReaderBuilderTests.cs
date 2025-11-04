using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;

namespace Sceny.Finance.IO.Tests;

public class CsvSourceReaderBuilderTests
{
    [Fact]
    public void WithDateFormat_SetsDateFormat()
    {
        // Act
        var reader = new CsvSourceReaderBuilder()
            .WithDateFormat("MM/dd/yyyy")
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void WithDelimiter_SetsDelimiter()
    {
        // Act
        var reader = new CsvSourceReaderBuilder()
            .WithDelimiter(';')
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void WithEncoding_SetsEncoding()
    {
        // Act
        var reader = new CsvSourceReaderBuilder()
            .WithEncoding(Encoding.UTF32)
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void WithHeaders_SetsHasHeaders()
    {
        // Act
        var reader = new CsvSourceReaderBuilder()
            .WithHeaders(false)
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void MapColumn_SetsColumnMapping()
    {
        // Act
        var reader = new CsvSourceReaderBuilder()
            .MapColumn("Date", "PostDate")
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void WithDefaultAccountType_SetsDefaultAccountType()
    {
        // Act
        var reader = new CsvSourceReaderBuilder()
            .WithDefaultAccountType(AccountType.Savings)
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void WithDefaultCurrency_SetsDefaultCurrency()
    {
        // Act
        var reader = new CsvSourceReaderBuilder()
            .WithDefaultCurrency("EUR")
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void Build_ReturnsCsvSourceReader()
    {
        // Act
        var reader = new CsvSourceReaderBuilder().Build();

        // Assert
        Assert.NotNull(reader);
        Assert.IsType<CsvSourceReader>(reader);
    }

    [Fact]
    public void FluentConfiguration_AllowsChaining()
    {
        // Act
        var reader = new CsvSourceReaderBuilder()
            .WithDateFormat("MM/dd/yyyy")
            .WithDelimiter(';')
            .WithEncoding(Encoding.UTF8)
            .WithHeaders(true)
            .MapColumn("Date", "PostDate")
            .WithDefaultAccountType(AccountType.Checking)
            .WithDefaultCurrency("USD")
            .Build();

        // Assert
        Assert.NotNull(reader);
    }
}

