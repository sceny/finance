using System.Linq;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;
using Sceny.Finance.IO.Plugin.File.Ofx;
using Sceny.Finance.IO.Sources;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO.Tests;

public class CopyExtensionsTests
{
    [Fact]
    public async Task CopyAccountsToAsync_CopiesAccountsFromReaderToWriter()
    {
        // Arrange
        var inputCsv = "AccountId,AccountName\nACC001,Test Account";
        var reader = FinanceReader.FromString(inputCsv).AsCsv();
        var outputSb = new StringBuilder();
        var writer = FinanceWriter.ToString(outputSb).AsCsv();

        // Act
        await reader.CopyAccountsToAsync(writer);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = outputSb.ToString();
        Assert.Contains("AccountId", result);
        Assert.Contains("ACC001", result);
        Assert.Contains("Test Account", result);
    }

    [Fact]
    public async Task CopyTransactionsToAsync_CopiesTransactionsFromReaderToWriter()
    {
        // Arrange
        var inputCsv = "AccountId,Date,Amount,Description\nACC001,2024-01-15,100.50,Test Transaction";
        var reader = FinanceReader.FromString(inputCsv).AsCsv();
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        var outputSb = new StringBuilder();
        var writer = FinanceWriter.ToString(outputSb).AsCsv();

        // Act
        await reader.CopyTransactionsToAsync(account, writer);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = outputSb.ToString();
        Assert.Contains("AccountId", result);
        Assert.Contains("Date", result);
        Assert.Contains("Amount", result);
        Assert.Contains("100.50", result);
    }

    [Fact]
    public async Task CopyAllToAsync_CopiesAccountsAndTransactions()
    {
        // Arrange
        var inputCsv = "AccountId,Date,Amount,Description\nACC001,2024-01-15,100.50,Test Transaction";
        var reader = FinanceReader.FromString(inputCsv).AsCsv();
        var outputSb = new StringBuilder();
        var writer = FinanceWriter.ToString(outputSb).AsCsv();

        // Act
        await reader.CopyAllToAsync(writer);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = outputSb.ToString();
        Assert.Contains("ACC001", result);
        Assert.Contains("100.50", result);
    }

    [Fact]
    public async Task CopyAccountsToAsync_WithFormatConversion_ConvertsFormat()
    {
        // Arrange
        var inputCsv = "AccountId,AccountName\nACC001,Test Account";
        var reader = FinanceReader.FromString(inputCsv).AsCsv();
        var outputSb = new StringBuilder();
        var writer = FinanceWriter.ToString(outputSb).AsOfx();

        // Act
        await reader.CopyAccountsToAsync(writer);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = outputSb.ToString();
        Assert.Contains("<OFX>", result);
        Assert.Contains("<ACCTID>ACC001</ACCTID>", result);
    }

    [Fact]
    public async Task CopyTransactionsToAsync_WithFormatConversion_ConvertsFormat()
    {
        // Arrange
        var inputCsv = "AccountId,Date,Amount,Description\nACC001,2024-01-15,100.50,Test";
        var reader = FinanceReader.FromString(inputCsv).AsCsv();
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        var outputSb = new StringBuilder();
        var writer = FinanceWriter.ToString(outputSb).AsOfx();

        // Act
        await reader.CopyTransactionsToAsync(account, writer);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = outputSb.ToString();
        Assert.Contains("<OFX>", result);
        Assert.Contains("<STMTTRN>", result);
        Assert.Contains("100.50", result);
    }

    [Fact]
    public async Task CopyAllToAsync_WithFormatConversion_ConvertsFormat()
    {
        // Arrange
        var inputCsv = "AccountId,Date,Amount,Description\nACC001,2024-01-15,100.50,Test";
        var reader = FinanceReader.FromString(inputCsv).AsCsv();
        var outputSb = new StringBuilder();
        var writer = FinanceWriter.ToString(outputSb).AsOfx();

        // Act
        await reader.CopyAllToAsync(writer);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = outputSb.ToString();
        Assert.Contains("<OFX>", result);
        Assert.Contains("<ACCTID>ACC001</ACCTID>", result);
        Assert.Contains("<STMTTRN>", result);
    }

    [Fact]
    public async Task CopyAccountsToAsync_WithCancellation_Throws()
    {
        // Arrange
        var inputCsv = "AccountId,AccountName\nACC001,Test Account";
        var reader = FinanceReader.FromString(inputCsv).AsCsv();
        var outputSb = new StringBuilder();
        var writer = FinanceWriter.ToString(outputSb).AsCsv();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await reader.CopyAccountsToAsync(writer, cts.Token);
        });
    }

    [Fact]
    public async Task CopyTransactionsToAsync_WithCancellation_Throws()
    {
        // Arrange
        var inputCsv = "AccountId,Date,Amount\nACC001,2024-01-15,100.50";
        var reader = FinanceReader.FromString(inputCsv).AsCsv();
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        var outputSb = new StringBuilder();
        var writer = FinanceWriter.ToString(outputSb).AsCsv();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await reader.CopyTransactionsToAsync(account, writer, cts.Token);
        });
    }

    [Fact]
    public async Task CopyAllToAsync_WithCancellation_Throws()
    {
        // Arrange
        var inputCsv = "AccountId,Date,Amount\nACC001,2024-01-15,100.50";
        var reader = FinanceReader.FromString(inputCsv).AsCsv();
        var outputSb = new StringBuilder();
        var writer = FinanceWriter.ToString(outputSb).AsCsv();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await reader.CopyAllToAsync(writer, cts.Token);
        });
    }

    [Fact]
    public async Task CopyAccountsToAsync_WithNullReader_ThrowsArgumentNullException()
    {
        // Arrange
        ConfiguredReader? reader = null;
        var outputSb = new StringBuilder();
        var writer = FinanceWriter.ToString(outputSb).AsCsv();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await reader!.CopyAccountsToAsync(writer);
        });
    }

    [Fact]
    public async Task CopyAccountsToAsync_WithNullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        var inputCsv = "AccountId,AccountName\nACC001,Test";
        var reader = FinanceReader.FromString(inputCsv).AsCsv();
        ConfiguredWriter? writer = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await reader.CopyAccountsToAsync(writer!);
        });
    }

    [Fact]
    public async Task CopyToAsync_WithSourceBuilderAndTargetBuilder_CopiesRawBytes()
    {
        // Arrange
        var sourceData = Encoding.UTF8.GetBytes("test data");
        var source = FinanceReader.FromBytes(sourceData);
        var targetSb = new StringBuilder();
        var target = FinanceWriter.ToString(targetSb);

        // Act
        await source.CopyToAsync(target);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = targetSb.ToString();
        Assert.Contains("test data", result);
    }

    [Fact]
    public async Task CopyToAsync_WithMemorySourceAndStringTarget_CopiesRawBytes()
    {
        // Arrange
        var sourceData = Encoding.UTF8.GetBytes("test data");
        var source = new MemorySource(sourceData);
        var targetSb = new StringBuilder();
        var target = new StringTarget(targetSb);

        // Act
        await source.CopyToAsync(target);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = targetSb.ToString();
        Assert.Contains("test data", result);
    }

    [Fact]
    public async Task CopyToAsync_WithStringSourceAndMemoryTarget_CopiesRawBytes()
    {
        // Arrange
        var source = new StringSource("test data");
        var bufferWriter = new System.Buffers.ArrayBufferWriter<byte>();
        var target = new MemoryTarget(bufferWriter);

        // Act
        await source.CopyToAsync(target);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        Assert.True(bufferWriter.WrittenCount > 0);
    }

    [Fact]
    public async Task CopyToAsync_WithFileSourceAndFileTarget_CopiesRawBytes()
    {
        // Arrange
        var sourceFile = Path.GetTempFileName();
        var targetFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(sourceFile, "test data");
            var source = new FileSource(sourceFile);
            var target = new FileTarget(targetFile);

            // Act
            await source.CopyToAsync(target);
            await TestHelpers.WaitForAsyncWrites();

            // Assert
            var result = File.ReadAllText(targetFile);
            Assert.Contains("test data", result);
        }
        finally
        {
            try { File.Delete(sourceFile); } catch { }
            try { File.Delete(targetFile); } catch { }
        }
    }

    [Fact]
    public async Task CopyToAsync_WithStreamSourceAndStreamTarget_CopiesRawBytes()
    {
        // Arrange
        var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes("test data"));
        var targetStream = new MemoryStream();
        var source = new StreamSource(sourceStream);
        var target = new StreamTarget(targetStream);

        // Act
        await source.CopyToAsync(target);
        await TestHelpers.WaitForAsyncWrites();
        targetStream.Position = 0;

        // Assert
        var reader = new StreamReader(targetStream);
        var result = reader.ReadToEnd();
        Assert.Contains("test data", result);
    }

    [Fact]
    public async Task CopyToAsync_WithNullSourceBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        SourceBuilder<MemorySource>? source = null;
        var targetSb = new StringBuilder();
        var target = FinanceWriter.ToString(targetSb);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await source!.CopyToAsync(target);
        });
    }

    [Fact]
    public async Task CopyToAsync_WithNullTargetBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        var sourceData = Encoding.UTF8.GetBytes("test");
        var source = FinanceReader.FromBytes(sourceData);
        TargetBuilder<StringTarget>? target = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await source.CopyToAsync(target!);
        });
    }

    [Fact]
    public async Task CopyToAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        ISource? source = null;
        var targetSb = new StringBuilder();
        var target = new StringTarget(targetSb);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await source!.CopyToAsync(target);
        });
    }

    [Fact]
    public async Task CopyToAsync_WithNullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        var sourceData = Encoding.UTF8.GetBytes("test");
        var source = new MemorySource(sourceData);
        ITarget? target = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await source.CopyToAsync(target!);
        });
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithFileSource_ReturnsPipeReader()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test data");
            var source = new FileSource(tempFile);

            // Act
            var reader = await source.GetPipeReaderAsync();

            // Assert
            Assert.NotNull(reader);
            await reader.CompleteAsync();
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithStreamSource_ReturnsPipeReader()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test data"));
        var source = new StreamSource(stream);

        // Act
        var reader = await source.GetPipeReaderAsync();

        // Assert
        Assert.NotNull(reader);
        await reader.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithMemorySource_ReturnsPipeReader()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test data");
        var source = new MemorySource(data);

        // Act
        var reader = await source.GetPipeReaderAsync();

        // Assert
        Assert.NotNull(reader);
        await reader.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithStringSource_ReturnsPipeReader()
    {
        // Arrange
        var source = new StringSource("test data");

        // Act - Use extension method explicitly
        var reader = await CopyExtensions.GetPipeReaderAsync(source);

        // Assert
        Assert.NotNull(reader);
        await reader.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithMemorySource_ThroughExtension_ReturnsPipeReader()
    {
        // Arrange
        var data = Encoding.UTF8.GetBytes("test data");
        var source = new MemorySource(data);

        // Act - Use extension method to hit the else branch
        var reader = await CopyExtensions.GetPipeReaderAsync(source);

        // Assert
        Assert.NotNull(reader);
        await reader.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        ISource? source = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await source!.GetPipeReaderAsync();
        });
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithFileTarget_ReturnsPipeWriter()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var target = new FileTarget(tempFile);

            // Act
            var writer = await target.GetPipeWriterAsync();

            // Assert
            Assert.NotNull(writer);
            await writer.CompleteAsync();
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithStreamTarget_ReturnsPipeWriter()
    {
        // Arrange
        var stream = new MemoryStream();
        var target = new StreamTarget(stream);

        // Act
        var writer = await target.GetPipeWriterAsync();

        // Assert
        Assert.NotNull(writer);
        await writer.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithMemoryTarget_ReturnsPipeWriter()
    {
        // Arrange
        var bufferWriter = new System.Buffers.ArrayBufferWriter<byte>();
        var target = new MemoryTarget(bufferWriter);

        // Act
        var writer = await target.GetPipeWriterAsync();

        // Assert
        Assert.NotNull(writer);
        await writer.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithMemoryTarget_ThroughExtension_ReturnsPipeWriter()
    {
        // Arrange
        var bufferWriter = new System.Buffers.ArrayBufferWriter<byte>();
        var target = new MemoryTarget(bufferWriter);

        // Act - Use extension method to hit the else branch
        var writer = await CopyExtensions.GetPipeWriterAsync(target);

        // Assert
        Assert.NotNull(writer);
        await writer.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithStringTarget_ReturnsPipeWriter()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);

        // Act
        var writer = await target.GetPipeWriterAsync();

        // Assert
        Assert.NotNull(writer);
        await writer.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithStringTarget_ThroughExtension_ReturnsPipeWriter()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);

        // Act - Use extension method to hit the else branch
        var writer = await CopyExtensions.GetPipeWriterAsync(target);

        // Assert
        Assert.NotNull(writer);
        await writer.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithFileTarget_ThroughExtension_ReturnsPipeWriter()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var target = new FileTarget(tempFile);

            // Act - Use extension method to hit FileTarget branch
            var writer = await CopyExtensions.GetPipeWriterAsync(target);

            // Assert
            Assert.NotNull(writer);
            await writer.CompleteAsync();
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithStreamTarget_ThroughExtension_ReturnsPipeWriter()
    {
        // Arrange
        var stream = new MemoryStream();
        var target = new StreamTarget(stream);

        // Act - Use extension method to hit StreamTarget branch
        var writer = await CopyExtensions.GetPipeWriterAsync(target);

        // Assert
        Assert.NotNull(writer);
        await writer.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithFileSource_ThroughExtension_ReturnsPipeReader()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "test data");
            var source = new FileSource(tempFile);

            // Act - Use extension method to hit FileSource branch
            var reader = await CopyExtensions.GetPipeReaderAsync(source);

            // Assert
            Assert.NotNull(reader);
            await reader.CompleteAsync();
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task GetPipeReaderAsync_WithStreamSource_ThroughExtension_ReturnsPipeReader()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("test data"));
        var source = new StreamSource(stream);

        // Act - Use extension method to hit StreamSource branch
        var reader = await CopyExtensions.GetPipeReaderAsync(source);

        // Assert
        Assert.NotNull(reader);
        await reader.CompleteAsync();
    }

    [Fact]
    public async Task GetPipeWriterAsync_WithNullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        ITarget? target = null;

        // Act & Assert
        // ArgumentNullException.ThrowIfNull throws synchronously, but we need to await to get the exception
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await CopyExtensions.GetPipeWriterAsync(target!, CancellationToken.None);
        });
        Assert.Equal("target", exception.ParamName);
    }
}

