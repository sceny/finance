# Sceny.Finance.IO Project Overview

## Project Overview
This is a zero-allocation financial data I/O library that uses System.IO.Pipelines for high-performance streaming. The library provides a fluent API for reading financial data from various sources (files, streams, HTTP, SignalR, message buses) and parsing different formats (CSV, OFX, etc.).

## Architecture Principles

### 1. Source/Format Separation
- **Sources** (ISource): Represent WHERE data comes from (FileSource, StreamSource, MemorySource, StringSource, HttpSource, SignalRSource, MessageBusSource)
- **Formats/Readers** (ISourceReader): Represent HOW data is parsed (CsvSourceReader, OfxSourceReader, etc.)
- **Core Library**: Format-agnostic - only knows about ISource and ISourceReader
- **Provider Projects**: Format-specific extensions (e.g., Sceny.Finance.IO.Plugin.File) expose format-specific methods via extension methods

### 2. Fluent API Pattern
- **Entry Point**: `FinanceReader` static class provides `FromFile()`, `FromStream()`, `FromBytes()`, `FromString()`
- **Source Builder**: `SourceBuilder<T>` is format-agnostic base class
- **Format Extensions**: Provider projects expose `AsCsv()`, `AsOfx()`, etc. as extension methods on `SourceBuilder<T>`
- **Configured Reader**: `ConfiguredReader` combines source + reader to provide `GetAccountsAsync()` and `GetTransactionsAsync()`

### 3. Zero-Allocation Requirements
- Use `System.IO.Pipelines` for all I/O operations
- Use `ReadOnlySpan<T>` and `ReadOnlyMemory<T>` for zero-allocation access
- Use `ReadOnlyString` for string data in domain models (Account, Transaction)
- Pool buffers using `ArrayPool<T>` when allocations are necessary
- Use `GetSpan()` and `Advance()` pattern for writing to pipes
- Avoid string allocations in hot paths
- Verify zero-allocation with `MemoryDiagnoser` in benchmarks

### 4. Folder Organization
- **Models/**: Domain models (Account, Transaction, AccountType, TransactionType)
- **Sources/**: Source implementations (FileSource, StreamSource, MemorySource, StringSource)
- **Root**: Core abstractions (ISource, ISourceReader, FinanceReader, SourceBuilder, ConfiguredReader, Utilities)
- **Provider Projects**: Format-specific implementations (Csv/, Ofx/) with extension methods

### 5. Naming Conventions
- **Sources**: `*Source` suffix (FileSource, StreamSource, etc.)
- **Readers**: `*SourceReader` suffix (CsvSourceReader, OfxSourceReader)
- **Options**: `*Options` suffix (CsvOptions, OfxOptions)
- **Builders**: `*SourceReaderBuilder` suffix (CsvSourceReaderBuilder, OfxSourceReaderBuilder)
- **Extensions**: `*SourceBuilderExtensions` suffix (CsvSourceBuilderExtensions, OfxSourceBuilderExtensions)
- **Namespaces**: `Sceny.Finance.IO` for core, `Sceny.Finance.IO.Plugin.File.*` for providers

## File Organization

### Core Library Structure
```
Sceny.Finance.IO.Core/
  - FinanceReader.cs (entry point)
  - ISourceReader.cs (format parser interface)
  - ISource.cs (source abstraction)
  - SourceBuilder.cs (base builder)
  - ConfiguredReader.cs (source + reader wrapper)
  - Utilities.cs (low-level helpers)
  - Models/
    - Account.cs
    - Transaction.cs
    - AccountType.cs
    - TransactionType.cs
  - Sources/
    - FileSource.cs
    - StreamSource.cs
    - MemorySource.cs
    - StringSource.cs
```

### Provider Project Structure
```
Sceny.Finance.IO.Plugin.File/
  - Formats/
    - Csv/
      - CsvSourceReader.cs
      - CsvOptions.cs
      - CsvSourceReaderBuilder.cs
      - CsvSourceBuilderExtensions.cs (extension methods)
    - Ofx/
      - OfxSourceReader.cs
      - OfxOptions.cs
      - OfxSourceReaderBuilder.cs
      - OfxSourceBuilderExtensions.cs (extension methods)
```

