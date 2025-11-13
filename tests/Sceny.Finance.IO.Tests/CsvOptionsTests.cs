using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;

namespace Sceny.Finance.IO.Tests;

public class CsvOptionsTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var options = new CsvOptions();

        // Assert
        Assert.Equal(',', options.Delimiter);
        Assert.Equal("yyyy-MM-dd", options.DateFormat);
        Assert.Equal(Encoding.UTF8, options.Encoding);
        Assert.True(options.HasHeaders);
        Assert.Equal(AccountType.Other, options.DefaultAccountType);
        Assert.Equal("USD", options.DefaultCurrency);
        Assert.NotNull(options.ColumnMapping);
    }

    [Fact]
    public void ColumnMapping_ContainsDefaultMappings()
    {
        // Act
        var options = new CsvOptions();

        // Assert
        Assert.True(options.ColumnMapping.ContainsKey("AccountId"));
        Assert.True(options.ColumnMapping.ContainsKey("AccountName"));
        Assert.True(options.ColumnMapping.ContainsKey("Date"));
        Assert.True(options.ColumnMapping.ContainsKey("Amount"));
        Assert.True(options.ColumnMapping.ContainsKey("Description"));
        Assert.True(options.ColumnMapping.ContainsKey("Type"));
        Assert.True(options.ColumnMapping.ContainsKey("Reference"));
    }

    [Fact]
    public void ColumnMapping_CanBeModified()
    {
        // Arrange
        var options = new CsvOptions();

        // Act
        options.ColumnMapping["Date"] = "PostDate";

        // Assert
        Assert.Equal("PostDate", options.ColumnMapping["Date"]);
    }

    [Fact]
    public void Delimiter_CanBeSet()
    {
        // Arrange
        var options = new CsvOptions();

        // Act
        options.Delimiter = ';';

        // Assert
        Assert.Equal(';', options.Delimiter);
    }

    [Fact]
    public void DateFormat_CanBeSet()
    {
        // Arrange
        var options = new CsvOptions();

        // Act
        options.DateFormat = "MM/dd/yyyy";

        // Assert
        Assert.Equal("MM/dd/yyyy", options.DateFormat);
    }

    [Fact]
    public void Encoding_CanBeSet()
    {
        // Arrange
        var options = new CsvOptions();

        // Act
        options.Encoding = Encoding.UTF32;

        // Assert
        Assert.Equal(Encoding.UTF32, options.Encoding);
    }

    [Fact]
    public void HasHeaders_CanBeSet()
    {
        // Arrange
        var options = new CsvOptions();

        // Act
        options.HasHeaders = false;

        // Assert
        Assert.False(options.HasHeaders);
    }

    [Fact]
    public void DefaultAccountType_CanBeSet()
    {
        // Arrange
        var options = new CsvOptions();

        // Act
        options.DefaultAccountType = AccountType.Savings;

        // Assert
        Assert.Equal(AccountType.Savings, options.DefaultAccountType);
    }

    [Fact]
    public void DefaultCurrency_CanBeSet()
    {
        // Arrange
        var options = new CsvOptions();

        // Act
        options.DefaultCurrency = "eur";

        // Assert
        Assert.Equal("EUR", options.DefaultCurrency);
    }

    [Fact]
    public void DateFormat_WithNull_Throws()
    {
        var options = new CsvOptions();
        Assert.Throws<ArgumentException>(() => options.DateFormat = " ");
    }

    [Fact]
    public void DefaultCurrency_WithNull_Throws()
    {
        var options = new CsvOptions();
        Assert.Throws<ArgumentException>(() => options.DefaultCurrency = "");
    }

    [Fact]
    public void Encoding_WithNull_DefaultsToUtf8()
    {
        var options = new CsvOptions
        {
            Encoding = null!
        };

        Assert.Equal(Encoding.UTF8, options.Encoding);
    }
}

