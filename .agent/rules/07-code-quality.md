---
trigger: glob
globs: *.cs
---

# Code Quality

## DRY Principle
- No duplicate `CreatePipeReader` methods - use `Utilities` helpers
- Single source of truth for PipeReader creation
- Shared utilities for common operations

## Clean Code
- Remove unused imports
- Remove obsolete code
- Remove trailing empty lines
- Keep files concise - split into smaller modules if needed

## Naming Conventions

### Principles for Good Names
- **Describe purpose, not implementation**: Names should explain WHAT the code does, not HOW it does it
- **Be specific, not generic**: Avoid generic names like `data`, `value`, `item` - use descriptive names like `accountId`, `transactionAmount`, `csvContent`
- **Use domain terminology**: Leverage business/financial terms that match the problem domain (Account, Transaction, Amount, etc.)
- **Avoid abbreviations**: Prefer full words over abbreviations unless the abbreviation is widely understood (e.g., `Http` is acceptable, `Acct` is not)
- **Test names describe behavior**: Test method names should describe what functionality is being tested, not which branch is being covered
  - ✅ Good: `GetAccountsAsync_WithInvalidAccountType_UsesDefault`
  - ❌ Bad: `GetAccountsAsync_WithInvalidAccountType_Branch_Coverage`

### Naming Patterns

#### Types and Classes
- **Sources**: `*Source` suffix (FileSource, StreamSource, etc.)
- **Readers**: `*SourceReader` suffix (CsvSourceReader, OfxSourceReader)
- **Options**: `*Options` suffix (CsvOptions, OfxOptions)
- **Builders**: `*SourceReaderBuilder` suffix (CsvSourceReaderBuilder, OfxSourceReaderBuilder)
- **Extensions**: `*SourceBuilderExtensions` suffix (CsvSourceBuilderExtensions, OfxSourceBuilderExtensions)
- Use PascalCase for all public types, classes, interfaces, and methods

#### Methods and Functions
- Use verb-noun pattern for methods that perform actions: `GetAccountsAsync()`, `ParseTransaction()`, `ReadSequenceAsync()`
- Use descriptive names that indicate purpose: `CreatePipeReaderFromFileAsync()` not `CreateReader()`
- Boolean methods/properties should be questions or assertions: `IsValid()`, `HasHeaders`, `CanParse()`

#### Variables and Parameters
- Use camelCase for local variables and parameters
- Use descriptive names that explain the variable's purpose: `accountId` not `id`, `transactionAmount` not `amt`
- Use meaningful names for loop variables when context matters: `account` over `item` when iterating accounts
- Use `_` prefix for private fields: `_options`, `_filePath`

#### Test Names
- Follow the pattern: `MethodName_Scenario_ExpectedBehavior`
- Group related tests by context using comments (e.g., `// Line Ending Handling`, `// Quoted Field Handling`)
- Test names should describe real functionality being tested, not implementation details
- Avoid "branch coverage" or "tests branch" in test names - focus on what behavior is being verified

### Examples

```csharp
// ✅ Good - descriptive, purpose-driven
public async IAsyncEnumerable<Account> GetAccountsAsync(PipeReader reader, CancellationToken cancellationToken = default)
{
    var csvContent = await Utilities.ReadCompleteSequenceAsync(reader, cancellationToken);
    var accountLines = SplitLines(csvContent);
    // ...
}

// ❌ Bad - generic, unclear purpose
public async IAsyncEnumerable<Account> Get(PipeReader r, CancellationToken ct = default)
{
    var data = await Utilities.Read(r, ct);
    var items = Split(data);
    // ...
}
```

## Comments and Documentation

### When to Comment
- **Public APIs**: All public classes, methods, and properties should have XML documentation comments
- **Complex logic**: Explain WHY code does something unusual, not WHAT it does (the code should be self-explanatory)
- **Business rules**: Document domain-specific rules and constraints
- **Non-obvious behavior**: Explain side effects, performance characteristics, or edge cases
- **Grouping context in tests**: Use comments to group related tests by functionality (e.g., `// Line Ending Handling`, `// Date Parsing`)

### When NOT to Comment
- ❌ Don't comment obvious code: `// Increment counter` above `counter++`
- ❌ Don't restate the code: `// Gets the account ID` above `account.Id`
- ❌ Don't use comments as excuses for bad code - fix the code instead
- ❌ Don't comment out code - delete it (version control keeps history)

### Comment Style
- Use XML documentation comments (`///`) for public APIs
- Use `//` for inline explanations of complex logic
- Use `// Section Name` comments to group related functionality in test files
- Keep comments concise and focused on the "why" not the "what"

### XML Documentation Examples

```csharp
/// <summary>
/// Reads accounts from a CSV-formatted source.
/// </summary>
/// <param name="reader">The pipe reader containing CSV data.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>An async enumerable of parsed accounts.</returns>
/// <remarks>
/// This method handles various line endings (CR, LF, CRLF) and quoted fields automatically.
/// Empty lines are skipped during parsing.
/// </remarks>
public async IAsyncEnumerable<Account> GetAccountsAsync(
    PipeReader reader,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
```

### Inline Comment Examples

```csharp
// ✅ Good - explains WHY, provides context
// OFX dates can be in multiple formats: date-only (yyyyMMdd) or datetime (yyyyMMddHHmmss)
// We need to handle both cases to support different OFX versions
if (dateString.Length >= 14)
{
    // Attempt to parse as datetime with time component
    return DateTime.ParseExact(dateString.Substring(0, 14), "yyyyMMddHHmmss", null);
}

// ❌ Bad - restates the obvious
// Check if date string length is 14 or more
if (dateString.Length >= 14)
{
    // Parse as datetime
    return DateTime.ParseExact(dateString.Substring(0, 14), "yyyyMMddHHmmss", null);
}
```

### Test Comment Examples

```csharp
// ✅ Good - groups related tests by functionality
// Line Ending Handling
[Fact]
public async Task GetAccountsAsync_WithCarriageReturnOnly_HandlesCorrectly()
{
    // ...
}

[Fact]
public async Task GetAccountsAsync_WithCarriageReturnLineFeed_HandlesCorrectly()
{
    // ...
}

// Quoted Field Handling
[Fact]
public async Task GetAccountsAsync_WithQuotedFieldAtEnd_HandlesCorrectly()
{
    // ...
}
```

## Compilation
- All code must compile without errors or warnings
- No test failures allowed
- Maintain existing code patterns and conventions

## Summary

This library provides a clean, extensible, zero-allocation API for reading financial data. The architecture separates concerns (sources vs formats), uses extension methods for extensibility, and maintains strict zero-allocation requirements. All code must follow these patterns and requirements to ensure consistency, performance, and maintainability.

**Key principles**: Names should describe purpose and functionality, not implementation details. Comments should explain WHY, not WHAT. Test names should describe behavior being verified, not branch coverage.
