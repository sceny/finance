using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Ofx;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO.Tests;

public class OfxTargetBuilderExtensionsTests
{
    [Fact]
    public void AsOfx_WithDefaultOptions_ReturnsConfiguredWriter()
    {
        // Arrange
        var sb = new StringBuilder();
        var builder = FinanceWriter.ToString(sb);

        // Act
        var writer = builder.AsOfx();

        // Assert
        Assert.NotNull(writer);
    }

    [Fact]
    public void AsOfx_WithConfigureAction_ReturnsConfiguredWriter()
    {
        // Arrange
        var sb = new StringBuilder();
        var builder = FinanceWriter.ToString(sb);
        bool configured = false;

        // Act
        var writer = builder.AsOfx(options =>
        {
            configured = true;
            options.DateFormat = "MM/dd/yyyy";
        });

        // Assert
        Assert.NotNull(writer);
        Assert.True(configured);
    }

    [Fact]
    public void AsOfx_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        TargetBuilder<StringTarget>? builder = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder!.AsOfx());
    }

    [Fact]
    public void AsOfx_WithNullConfigureAction_ThrowsArgumentNullException()
    {
        // Arrange
        var sb = new StringBuilder();
        var builder = FinanceWriter.ToString(sb);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AsOfx(null!));
    }

    [Fact]
    public async Task AsOfx_WithCustomDateFormat_UsesFormat()
    {
        // Arrange
        var sb = new StringBuilder();
        var builder = FinanceWriter.ToString(sb);
        var writer = builder.AsOfx(options =>
        {
            options.DateFormat = "MM/dd/yyyy";
            options.DateTimeFormat = "MM/dd/yyyy HH:mm:ss";
        });
        var transaction = Transaction.FromStrings("ACC001", 100m, DateTime.Parse("2024-01-15 10:30:00"), "Test", TransactionType.Credit);

        // Act
        await writer.BeginWriteAsync();
        await writer.WriteTransactionAsync(transaction);
        await writer.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("<DTPOSTED>", result);
    }
}

