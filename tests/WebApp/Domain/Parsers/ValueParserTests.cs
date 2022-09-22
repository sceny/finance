using System;
using FluentAssertions;
using WebApp.Domain.Parsers;
using Xunit;

namespace Sceny.Finance.WebApp.Tests.Domain.Parsers;

public class ValueParserTests
{
    [Theory]
    [InlineData("M3D4Y2010", 2010, 03, 04)]
    public void Custom_date_parsing(
        string date,
        int expectedYear,
        int expectedMonth,
        int expectedDay)
    {
        // arrange
        var valueParser = new ValueParser(
            new(
                new[] { "\\MM\\Dd\\Yyyyy" }
            )
        );
        // act
        var result = valueParser.TryParseDate(date, out var parsedDate);
        // assert
        result.Should().BeTrue();
        parsedDate.Should().Be(new DateTime(expectedYear, expectedMonth, expectedDay));
    }
}