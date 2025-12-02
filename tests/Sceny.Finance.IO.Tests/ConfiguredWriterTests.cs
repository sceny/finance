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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);

        // Assert
        Assert.NotNull(configuredWriter);
    }

    [Fact]
    public void Constructor_WithNullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        var writer = new CsvSourceWriter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(null!, writer));
    }

    [Fact]
    public void Constructor_WithNullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, null!));
    }

    [Fact]
    public async Task WriteAccountsAsync_WithStringTarget_WritesAccounts()
    {
        // Arrange
        var sb = new StringBuilder();
        var target = new StringTarget(sb);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var accounts = new[]
        {
            Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", default(CsvAccountProperties))
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        var transactions = new[]
        {
            Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit, default(CsvTransactionProperties))
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", default(CsvAccountProperties));

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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var transaction = Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit, default(CsvTransactionProperties));

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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));

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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var transaction = Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit, default(CsvTransactionProperties));

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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);

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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);

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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var accounts = new[]
        {
            Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties))
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        var transactions = new[]
        {
            Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit, default(CsvTransactionProperties))
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var accounts = new[]
        {
            Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties))
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        var transactions = new[]
        {
            Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit, default(CsvTransactionProperties))
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
            var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
            var accounts = new[]
            {
                Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", default(CsvAccountProperties))
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var accounts = new[]
        {
            Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", default(CsvAccountProperties))
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var accounts = new[]
        {
            Account<CsvAccountProperties>.FromStrings("ACC001", "Test Account", AccountType.Checking, "USD", default(CsvAccountProperties))
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
            var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
            var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
            var transactions = new[]
            {
                Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit, default(CsvTransactionProperties))
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
            var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);

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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);

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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);

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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var accounts = new[]
        {
            Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties))
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));
        var transactions = new[]
        {
            Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Test", TransactionType.Credit, default(CsvTransactionProperties))
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);

        // Act
        await configuredWriter.BeginWriteAsync();
        // Call WriteAccountAsync multiple times - this should use the existing _pipeWriter
        // This tests the branch: if (_isInitialized && _pipeWriter != null) return _pipeWriter;
        var account1 = Account<CsvAccountProperties>.FromStrings("ACC001", "Test1", AccountType.Checking, "USD", default(CsvAccountProperties));
        var account2 = Account<CsvAccountProperties>.FromStrings("ACC002", "Test2", AccountType.Savings, "USD", default(CsvAccountProperties));
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var account = Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties));

        // Act
        await configuredWriter.BeginWriteAsync();
        // Call WriteTransactionAsync multiple times - this should use the existing _pipeWriter
        var tx1 = Transaction<CsvTransactionProperties>.FromStrings("ACC001", 100.50m, DateTime.Parse("2024-01-15"), "Tx1", TransactionType.Credit, default(CsvTransactionProperties));
        var tx2 = Transaction<CsvTransactionProperties>.FromStrings("ACC001", 50.25m, DateTime.Parse("2024-01-16"), "Tx2", TransactionType.Debit, default(CsvTransactionProperties));
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
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);

        // Act - This tests the else branch in GetOrCreatePipeWriterAsync for MemoryTarget
        var accounts = new[]
        {
            Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties))
        }.ToAsyncEnumerable();
        await configuredWriter.WriteAccountsAsync(accounts);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        Assert.True(bufferWriter.WrittenCount > 0);
    }

    [Fact]
    public async Task GetOrCreatePipeWriterAsync_WithFileTarget_UsesFileTargetBranch()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csv");
        try
        {
            var target = new FileTarget(tempFile);
            var writer = new CsvSourceWriter();
            var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
            var accounts = new[]
            {
                Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties))
            }.ToAsyncEnumerable();

            // Act - This tests the FileTarget branch in GetOrCreatePipeWriterAsync
            await configuredWriter.WriteAccountsAsync(accounts);
            await TestHelpers.WaitForAsyncWrites();

            // Assert
            Assert.True(File.Exists(tempFile));
            var content = await File.ReadAllTextAsync(tempFile);
            Assert.Contains("ACC001", content);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { }
        }
    }

    [Fact]
    public async Task GetOrCreatePipeWriterAsync_WithStreamTarget_UsesStreamTargetBranch()
    {
        // Arrange
        var stream = new MemoryStream();
        var target = new StreamTarget(stream);
        var writer = new CsvSourceWriter();
        var configuredWriter = new ConfiguredWriter<CsvAccountProperties, CsvTransactionProperties>(target, writer);
        var accounts = new[]
        {
            Account<CsvAccountProperties>.FromStrings("ACC001", "Test", AccountType.Checking, "USD", default(CsvAccountProperties))
        }.ToAsyncEnumerable();

        // Act - This tests the StreamTarget branch in GetOrCreatePipeWriterAsync
        await configuredWriter.WriteAccountsAsync(accounts);
        await TestHelpers.WaitForAsyncWrites();

        // Assert
        stream.Position = 0;
        var content = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Contains("ACC001", content);
    }

}

