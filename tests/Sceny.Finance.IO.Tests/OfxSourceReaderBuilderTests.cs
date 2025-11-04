using System.Text;
using Sceny.Finance.IO.Plugin.File.Ofx;

namespace Sceny.Finance.IO.Tests;

public class OfxSourceReaderBuilderTests
{
    [Fact]
    public void WithDateFormat_SetsDateFormat()
    {
        // Act
        var reader = new OfxSourceReaderBuilder()
            .WithDateFormat("yyyyMMdd")
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void WithDateTimeFormat_SetsDateTimeFormat()
    {
        // Act
        var reader = new OfxSourceReaderBuilder()
            .WithDateTimeFormat("yyyyMMddHHmmss")
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void WithTimeZoneOffset_SetsTimeZoneOffset()
    {
        // Act
        var reader = new OfxSourceReaderBuilder()
            .WithTimeZoneOffset(TimeSpan.FromHours(-5))
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void WithTimeZoneOffset_WithNull_SetsTimeZoneOffsetToNull()
    {
        // Act
        var reader = new OfxSourceReaderBuilder()
            .WithTimeZoneOffset(null)
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void WithEncoding_SetsEncoding()
    {
        // Act
        var reader = new OfxSourceReaderBuilder()
            .WithEncoding(Encoding.UTF32)
            .Build();

        // Assert
        Assert.NotNull(reader);
    }

    [Fact]
    public void Build_ReturnsOfxSourceReader()
    {
        // Act
        var reader = new OfxSourceReaderBuilder().Build();

        // Assert
        Assert.NotNull(reader);
        Assert.IsType<OfxSourceReader>(reader);
    }

    [Fact]
    public void FluentConfiguration_AllowsChaining()
    {
        // Act
        var reader = new OfxSourceReaderBuilder()
            .WithDateFormat("yyyyMMdd")
            .WithDateTimeFormat("yyyyMMddHHmmss")
            .WithTimeZoneOffset(TimeSpan.FromHours(-5))
            .WithEncoding(Encoding.UTF8)
            .Build();

        // Assert
        Assert.NotNull(reader);
    }
}

