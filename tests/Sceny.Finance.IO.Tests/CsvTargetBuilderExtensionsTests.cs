using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO.Tests;

public class CsvTargetBuilderExtensionsTests
{
    [Fact]
    public void AsCsv_WithDefaultOptions_ReturnsConfiguredWriter()
    {
        // Arrange
        var sb = new StringBuilder();
        var builder = FinanceWriter.ToString(sb);

        // Act
        var writer = builder.AsCsv();

        // Assert
        Assert.NotNull(writer);
    }

    [Fact]
    public void AsCsv_WithConfigureAction_ReturnsConfiguredWriter()
    {
        // Arrange
        var sb = new StringBuilder();
        var builder = FinanceWriter.ToString(sb);
        bool configured = false;

        // Act
        var writer = builder.AsCsv(options =>
        {
            configured = true;
            options.Delimiter = ';';
        });

        // Assert
        Assert.NotNull(writer);
        Assert.True(configured);
    }

    [Fact]
    public void AsCsv_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        TargetBuilder<StringTarget>? builder = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder!.AsCsv());
    }

    [Fact]
    public void AsCsv_WithNullConfigureAction_ThrowsArgumentNullException()
    {
        // Arrange
        var sb = new StringBuilder();
        var builder = FinanceWriter.ToString(sb);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AsCsv(null!));
    }

    [Fact]
    public async Task AsCsv_WithCustomOptions_UsesOptions()
    {
        // Arrange
        var sb = new StringBuilder();
        var builder = FinanceWriter.ToString(sb);
        var writer = builder.AsCsv(options =>
        {
            options.Delimiter = ';';
            options.HasHeaders = false;
        });
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        await writer.BeginWriteAsync();
        await writer.WriteAccountAsync(account);
        await writer.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.DoesNotContain("AccountId", result); // No headers
        Assert.Contains(";", result); // Uses semicolon delimiter
    }
}

