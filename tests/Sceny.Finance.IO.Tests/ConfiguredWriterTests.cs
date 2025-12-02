using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using Sceny.Finance.IO;
using Sceny.Finance.IO.Plugin.File.Csv;
using Sceny.Finance.IO.Targets;

namespace Sceny.Finance.IO.Tests;

public class ConfiguredWriterTests
{
    [Fact]
    public void Constructor_WithValidTargetAndWriter_DoesNotThrow()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();

        // Act
        var configuredWriter = new ConfiguredWriter(target, writer);

        // Assert
        Assert.NotNull(configuredWriter);
    }

    [Fact]
    public void Constructor_WithNullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        var writer = new CsvSourceWriter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConfiguredWriter(null!, writer));
    }

    [Fact]
    public void Constructor_WithNullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConfiguredWriter(target, null!));
    }

    [Fact]
    public async Task WriteAccountsAsync_WithStringTarget_WritesAccounts()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD")
        }.ToAsyncEnumerable();

        // Act
        await configuredWriter.WriteAccountsAsync(accounts);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("AccountId", result);
        Assert.Contains("ACC001", result);
        Assert.Contains("Test Account", result);
    }

    [Fact]
    public async Task WriteTransactionsAsync_WithStringTarget_WritesTransactions()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        var transactions = new[]
        {
            Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit)
        }.ToAsyncEnumerable();

        // Act
        await configuredWriter.WriteTransactionsAsync(account, transactions);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("AccountId", result);
        Assert.Contains("Date", result);
        Assert.Contains("Amount", result);
        Assert.Contains("100.50", result);
    }

    [Fact]
    public async Task BeginWriteAsync_ThenWriteAccountAsync_ThenEndWriteAsync_WritesCorrectly()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var account = Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD");

        // Act
        await configuredWriter.BeginWriteAsync();
        await configuredWriter.WriteAccountAsync(account);
        await configuredWriter.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("AccountId", result);
        Assert.Contains("ACC001", result);
    }

    [Fact]
    public async Task BeginWriteAsync_ThenWriteTransactionAsync_ThenEndWriteAsync_WritesCorrectly()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit);

        // Act
        await configuredWriter.BeginWriteAsync();
        await configuredWriter.WriteTransactionAsync(transaction);
        await configuredWriter.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        var result = sb.ToString();
        Assert.Contains("AccountId", result);
        Assert.Contains("Date", result);
        Assert.Contains("100.50", result);
    }

    [Fact]
    public async Task WriteAccountAsync_WithoutBeginWriteAsync_ThrowsInvalidOperationException()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await configuredWriter.WriteAccountAsync(account);
        });
    }

    [Fact]
    public async Task WriteTransactionAsync_WithoutBeginWriteAsync_ThrowsInvalidOperationException()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var transaction = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await configuredWriter.WriteTransactionAsync(transaction);
        });
    }

    [Fact]
    public async Task EndWriteAsync_WithoutBeginWriteAsync_ThrowsInvalidOperationException()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await configuredWriter.EndWriteAsync();
        });
    }

    [Fact]
    public async Task BeginWriteAsync_Twice_ThrowsInvalidOperationException()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);

        // Act
        await configuredWriter.BeginWriteAsync();

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await configuredWriter.BeginWriteAsync();
        });

        // Cleanup
        await configuredWriter.EndWriteAsync();
    }

    [Fact]
    public async Task WriteAccountsAsync_WhileManualWriteActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD")
        }.ToAsyncEnumerable();

        // Act
        await configuredWriter.BeginWriteAsync();

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await configuredWriter.WriteAccountsAsync(accounts);
        });

        // Cleanup
        await configuredWriter.EndWriteAsync();
    }

    [Fact]
    public async Task WriteTransactionsAsync_WhileManualWriteActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        var transactions = new[]
        {
            Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit)
        }.ToAsyncEnumerable();

        // Act
        await configuredWriter.BeginWriteAsync();

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await configuredWriter.WriteTransactionsAsync(account, transactions);
        });

        // Cleanup
        await configuredWriter.EndWriteAsync();
    }

    [Fact]
    public async Task WriteAccountsAsync_WithCancellation_Throws()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD")
        }.ToAsyncEnumerable();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await configuredWriter.WriteAccountsAsync(accounts, cts.Token);
        });
    }

    [Fact]
    public async Task WriteTransactionsAsync_WithCancellation_Throws()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        var transactions = new[]
        {
            Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit)
        }.ToAsyncEnumerable();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await configuredWriter.WriteTransactionsAsync(account, transactions, cts.Token);
        });
    }

    [Fact]
    public async Task WriteAccountsAsync_WithFileTarget_WritesToFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var target = new FileTarget(tempFile);
            var writer = new CsvSourceWriter();
            var configuredWriter = new ConfiguredWriter(target, writer);
            var accounts = new[]
            {
                Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD")
            }.ToAsyncEnumerable();

            // Act
            await configuredWriter.WriteAccountsAsync(accounts);
            await TestHelpers.WaitForAsyncWrites();

            // Assert
            var content = File.ReadAllText(tempFile);
            Assert.Contains("AccountId", content);
            Assert.Contains("ACC001", content);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task WriteAccountsAsync_WithStreamTarget_WritesToStream()
    {
        // Arrange
        var stream = new MemoryStream();
        var target = new StreamTarget(stream);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD")
        }.ToAsyncEnumerable();

        // Act
        await configuredWriter.WriteAccountsAsync(accounts);
        await TestHelpers.WaitForAsyncWrites();
        stream.Position = 0;

        // Assert
        var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        Assert.Contains("AccountId", content);
        Assert.Contains("ACC001", content);
    }

    [Fact]
    public async Task WriteAccountsAsync_WithMemoryTarget_WritesToMemory()
    {
        // Arrange
        var bufferWriter = new ArrayBufferWriter<byte>();
        var target = new MemoryTarget(bufferWriter);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD")
        }.ToAsyncEnumerable();

        // Act
        await configuredWriter.WriteAccountsAsync(accounts);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        Assert.True(bufferWriter.WrittenCount > 0);
    }

    [Fact]
    public async Task WriteTransactionsAsync_WithFileTarget_WritesToFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var target = new FileTarget(tempFile);
            var writer = new CsvSourceWriter();
            var configuredWriter = new ConfiguredWriter(target, writer);
            var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
            var transactions = new[]
            {
                Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit)
            }.ToAsyncEnumerable();

            // Act
            await configuredWriter.WriteTransactionsAsync(account, transactions);
            await TestHelpers.WaitForAsyncWrites();

            // Assert
            var content = File.ReadAllText(tempFile);
            Assert.Contains("AccountId", content);
            Assert.Contains("100.50", content);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task BeginWriteAsync_WithFileTarget_InitializesCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var target = new FileTarget(tempFile);
            var writer = new CsvSourceWriter();
            var configuredWriter = new ConfiguredWriter(target, writer);

            // Act
            await configuredWriter.BeginWriteAsync();
            await configuredWriter.EndWriteAsync();

            // Assert - Should not throw
            Assert.True(true);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task BeginWriteAsync_WithStreamTarget_InitializesCorrectly()
    {
        // Arrange
        var stream = new MemoryStream();
        var target = new StreamTarget(stream);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);

        // Act
        await configuredWriter.BeginWriteAsync();
        await configuredWriter.EndWriteAsync();

        // Assert - Should not throw
        Assert.True(true);
    }

    [Fact]
    public async Task BeginWriteAsync_WithMemoryTarget_InitializesCorrectly()
    {
        // Arrange
        var bufferWriter = new ArrayBufferWriter<byte>();
        var target = new MemoryTarget(bufferWriter);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);

        // Act
        await configuredWriter.BeginWriteAsync();
        await configuredWriter.EndWriteAsync();

        // Assert - Should not throw
        Assert.True(true);
    }

    [Fact]
    public async Task WriteAccountsAsync_WhenInitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD")
        }.ToAsyncEnumerable();

        // Act
        await configuredWriter.BeginWriteAsync();

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await configuredWriter.WriteAccountsAsync(accounts);
        });

        await configuredWriter.EndWriteAsync();
    }

    [Fact]
    public async Task WriteTransactionsAsync_WhenInitialized_ThrowsInvalidOperationException()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");
        var transactions = new[]
        {
            Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit)
        }.ToAsyncEnumerable();

        // Act
        await configuredWriter.BeginWriteAsync();

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await configuredWriter.WriteTransactionsAsync(account, transactions);
        });

        await configuredWriter.EndWriteAsync();
    }

    [Fact]
    public async Task GetOrCreatePipeWriterAsync_WhenInitialized_ReturnsExistingWriter()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);

        // Act
        await configuredWriter.BeginWriteAsync();
        // Call WriteAccountAsync multiple times - this should use the existing _pipeWriter
        // This tests the branch: if (_isInitialized && _pipeWriter != null) return _pipeWriter;
        var account1 = Account.FromStrings("ACC001", "Test1", AccountType.Checking, "USD");
        var account2 = Account.FromStrings("ACC002", "Test2", AccountType.Savings, "USD");
        await configuredWriter.WriteAccountAsync(account1);
        await configuredWriter.WriteAccountAsync(account2);
        
        // Assert - This tests that GetOrCreatePipeWriterAsync returns existing _pipeWriter when initialized
        await configuredWriter.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();
        
        var result = sb.ToString();
        Assert.Contains("ACC001", result);
        Assert.Contains("ACC002", result);
    }

    [Fact]
    public async Task GetOrCreatePipeWriterAsync_WhenInitialized_WithMultipleTransactions_ReturnsExistingWriter()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);
        var account = Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD");

        // Act
        await configuredWriter.BeginWriteAsync();
        // Call WriteTransactionAsync multiple times - this should use the existing _pipeWriter
        var tx1 = Transaction.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Tx1", TransactionType.Credit);
        var tx2 = Transaction.FromStrings("ACC001", 50.25m, DateTime.Parse("2024-01-16"), "Tx2", TransactionType.Debit);
        await configuredWriter.WriteTransactionAsync(tx1);
        await configuredWriter.WriteTransactionAsync(tx2);
        
        // Assert
        await configuredWriter.EndWriteAsync();
        await TestHelpers.WaitForAsyncWrites();
        
        var result = sb.ToString();
        Assert.Contains("100.50", result);
        Assert.Contains("50.25", result);
    }

    [Fact]
    public async Task GetOrCreatePipeWriterAsync_WithMemoryTarget_UsesElseBranch()
    {
        // Arrange
        var bufferWriter = new ArrayBufferWriter<byte>();
        var target = new MemoryTarget(bufferWriter);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter(target, writer);

        // Act - This tests the else branch in GetOrCreatePipeWriterAsync for MemoryTarget
        var accounts = new[]
        {
            Account.FromStrings("ACC001", "Test", AccountType.Checking, "USD")
        }.ToAsyncEnumerable();
        await configuredWriter.WriteAccountsAsync(accounts);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        Assert.True(bufferWriter.WrittenCount > 0);
    }

}

