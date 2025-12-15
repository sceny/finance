---
trigger: glob
globs: *.cs
---

# Code Patterns

**When writing code, ALWAYS follow these patterns:**

## Creating PipeReader (Zero-Allocation)

**ALWAYS use Utilities helpers - never create PipeReader directly:**
```csharp
// Use Utilities helpers for zero-allocation creation
var reader = Utilities.CreatePipeReaderFromString(content);
var reader = Utilities.CreatePipeReaderFromBytes(data);
var reader = await Utilities.CreatePipeReaderFromFileAsync(filePath);
var reader = await Utilities.CreatePipeReaderFromStreamAsync(stream);
```

## Using the Fluent API

**When writing tests or examples, use this API pattern:**
```csharp
// Simple usage
var accounts = await FinanceReader.FromFile("data.csv").AsCsv().GetAccountsAsync().ToListAsync();

// With configuration
var accounts = await FinanceReader.FromString(csv).AsCsv(options => options.Delimiter = ';').GetAccountsAsync().ToListAsync();

// From bytes
var transactions = await FinanceReader.FromBytes(data).AsOfx().GetTransactionsAsync(account).ToListAsync();
```

## Extension Methods in Provider Projects

**When creating new format providers, follow this pattern:**
```csharp
// Provider projects expose format-specific methods
public static class CsvSourceBuilderExtensions
{
    public static ConfiguredReader AsCsv<TSource>(this SourceBuilder<TSource> builder) where TSource : ISource
    {
        var reader = new CsvSourceReader();
        return new ConfiguredReader(builder.Source, reader);
    }
    
    public static ConfiguredReader AsCsv<TSource>(this SourceBuilder<TSource> builder, Action<CsvOptions> configure)
    {
        var options = new CsvOptions();
        configure(options);
        var reader = new CsvSourceReader(options);
        return new ConfiguredReader(builder.Source, reader);
    }
}
```

## Source Implementation Pattern

**When implementing a new ISource, follow this pattern:**
```csharp
public sealed class FileSource : ISource
{
    public PipeReader GetPipeReader(CancellationToken cancellationToken = default)
    {
        // For async sources, prefer GetPipeReaderAsync()
        return Utilities.CreatePipeReaderFromFileAsync(_filePath, cancellationToken).GetAwaiter().GetResult();
    }
    
    public async Task<PipeReader> GetPipeReaderAsync(CancellationToken cancellationToken = default)
    {
        return await Utilities.CreatePipeReaderFromFileAsync(_filePath, cancellationToken).ConfigureAwait(false);
    }
}
```

## Reader Implementation Pattern

**When implementing a new ISourceReader, follow this pattern:**
```csharp
public sealed class CsvSourceReader : ISourceReader
{
    public async IAsyncEnumerable<Account> GetAccountsAsync(
        PipeReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sequence = await Utilities.ReadCompleteSequenceAsync(reader, cancellationToken).ConfigureAwait(false);
        var text = Utilities.SequenceToString(sequence, _options.Encoding);
        // ... parse and yield accounts ...
        Utilities.AdvanceReader(reader, sequence);
    }
}
```

## Pre-Existing Code Patterns

### Domain Models
- Use `readonly struct` for Account and Transaction
- Use `ReadOnlyString` for all string fields
- Provide `FromStrings()` static methods for convenience
- Enums for AccountType and TransactionType

### Utilities
- Static helper class with zero-allocation methods
- Pooled buffer management
- Pipeline operations (ReadCompleteSequenceAsync, AdvanceReader, SequenceToString)
- PipeReader creation helpers

### Provider Projects
- Each format has its own namespace (Csv, Ofx)
- Options classes for configuration
- Builder classes for fluent configuration
- SourceReader implementations

## New Code Patterns (Added in This Refactoring)

### FinanceReader Entry Point
- Static class with `FromFile()`, `FromStream()`, `FromBytes()`, `FromString()`
- Returns `SourceBuilder<T>` for fluent chaining
- Format-agnostic - providers add format methods via extensions

### SourceBuilder
- Generic base class `SourceBuilder<TSource>`
- Format-agnostic - no format-specific methods
- Public `Source` property for extension methods
- Internal constructor - only FinanceReader creates instances

### ConfiguredReader
- Wraps ISource + ISourceReader
- Provides `GetAccountsAsync()` and `GetTransactionsAsync()`
- Handles async sources (FileSource, StreamSource) properly
- Public constructor for extension methods

### Source Implementations
- FileSource: File-based source with async support
- StreamSource: Stream-based source with async support
- MemorySource: In-memory byte data source
- StringSource: String-based source with encoding support

### Extension Methods
- Provider projects expose `AsCsv()`, `AsOfx()`, etc.
- Two overloads: default options and configured options
- Return `ConfiguredReader` for chaining
