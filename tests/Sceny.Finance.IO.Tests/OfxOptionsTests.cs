using System.Text;
using Sceny.Finance.IO.Plugin.File.Ofx;

namespace Sceny.Finance.IO.Tests;

public class OfxOptionsTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var options = new OfxOptions();

        // Assert
        Assert.Equal("yyyyMMdd", options.DateFormat);
        Assert.Equal("yyyyMMddHHmmss", options.DateTimeFormat);
        Assert.Null(options.TimeZoneOffset);
        Assert.Equal(Encoding.UTF8, options.Encoding);
    }

    [Fact]
    public void DateFormat_CanBeSet()
    {
        // Arrange
        var options = new OfxOptions();

        // Act
        options.DateFormat = "MM/dd/yyyy";

        // Assert
        Assert.Equal("MM/dd/yyyy", options.DateFormat);
    }

    [Fact]
    public void DateFormat_WithInvalidValue_Throws()
    {
        var options = new OfxOptions();
        Assert.Throws<ArgumentException>(() => options.DateFormat = " ");
    }

    [Fact]
    public void DateTimeFormat_CanBeSet()
    {
        // Arrange
        var options = new OfxOptions();

        // Act
        options.DateTimeFormat = "yyyyMMddHHmmss.fff";

        // Assert
        Assert.Equal("yyyyMMddHHmmss.fff", options.DateTimeFormat);
    }

    [Fact]
    public void DateTimeFormat_WithInvalidValue_Throws()
    {
        var options = new OfxOptions();
        Assert.Throws<ArgumentNullException>(() => options.DateTimeFormat = null!);
    }

    [Fact]
    public void TimeZoneOffset_CanBeSet()
    {
        // Arrange
        var options = new OfxOptions();

        // Act
        options.TimeZoneOffset = TimeSpan.FromHours(-5);

        // Assert
        Assert.Equal(TimeSpan.FromHours(-5), options.TimeZoneOffset);
    }

    [Fact]
    public void TimeZoneOffset_CanBeSetToNull()
    {
        // Arrange
        var options = new OfxOptions();
        options.TimeZoneOffset = TimeSpan.FromHours(-5);

        // Act
        options.TimeZoneOffset = null;

        // Assert
        Assert.Null(options.TimeZoneOffset);
    }

    [Fact]
    public void Encoding_CanBeSet()
    {
        // Arrange
        var options = new OfxOptions();

        // Act
        options.Encoding = Encoding.UTF32;

        // Assert
        Assert.Equal(Encoding.UTF32, options.Encoding);
    }

    [Fact]
    public void Encoding_WithNull_DefaultsToUtf8()
    {
        var options = new OfxOptions
        {
            Encoding = null!
        };

        Assert.Equal(Encoding.UTF8, options.Encoding);
    }
}

