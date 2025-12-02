# Complete Rules Analysis - All Completion Criteria, Standards, and Goals

## Executive Summary

This document provides a comprehensive analysis of all rules defined in `.cursor/rules/` covering completion criteria, standards, goals, coverage requirements, and benchmark specifications.

---

## 1. Completion Criteria (08-completion.md)

### Build Verification
**Requirement**: Build must succeed with no errors or warnings
- Command: `dotnet build`
- Action if failed: Fix all compilation errors and warnings

### Test Execution and Coverage
**Requirements**:
- ✅ **100% test pass rate** (all tests must pass)
- ✅ **Minimum 90% line coverage**
- ✅ **Minimum 90% branch coverage**
- Action if failed:
  - Fix failing tests
  - Add missing test cases to achieve coverage requirements
  - Verify all conditional branches are tested

### Benchmark Verification
**Requirement**: ⚠️ **All benchmarks must show zero allocations (0 B allocated)**
- Command: `dotnet run --project tests/Sceny.Finance.IO.Benchmarks`
- Verification: Check `MemoryDiagnoser` output for allocation metrics
- Action if failed:
  - Identify and fix allocation sources
  - Use `ArrayPool<T>` for necessary allocations
  - Verify use of `ReadOnlySpan<T>` and `ReadOnlyMemory<T>` in hot paths
  - Ensure `System.IO.Pipelines` is used correctly

### Completion Checklist
Before marking a task as complete:
- [ ] `dotnet build` succeeds without errors or warnings
- [ ] `dotnet test` passes with 100% test pass rate
- [ ] Test coverage meets 90% line coverage minimum
- [ ] Test coverage meets 90% branch coverage minimum
- [ ] **All benchmarks run without allocations (0 B allocated)** ⚠️
- [ ] All diagnostic code (`[#ai_diagnostic]`) has been removed
- [ ] Code follows project standards and patterns

---

## 2. Testing Requirements (03-testing.md)

### Test Coverage Requirements
- **Minimum 90% line coverage** for all new code
- **Minimum 90% branch coverage** for all new code
- **All tests MUST pass** - no exceptions (100% pass rate required)

### Test Scenarios Required
- Test both success and error scenarios
- Test all configuration options
- Test cancellation tokens
- Test edge cases (empty files, invalid data, etc.)
- Test all conditional branches (if/else, switch, ternary operators)
- Test all loop variations (early exit, continue, break conditions)

### Test Patterns
- Use `FinanceReader` API in tests (not direct ISourceReader usage)
- Remove duplicate `CreatePipeReader` methods - use `Utilities.CreatePipeReaderFromString()` or `FinanceReader.FromString()`
- Test both default and configured options
- Use `ToListAsync()` for assertions in tests

### Benchmark Requirements
- **All benchmarks must use `[MemoryDiagnoser]` to verify zero-allocation**
- Benchmarks should use `FinanceReader` API
- Use `FromBytes()` for byte array data in benchmarks

---

## 3. Performance Guidelines (04-performance.md)

### System.IO.Pipelines Usage
- Always use `Utilities.ReadCompleteSequenceAsync()` to read complete sequences
- Always call `Utilities.AdvanceReader()` after consuming data
- Use `ReadOnlySequence<byte>` for zero-allocation access
- Use `SequenceToString()` for multi-segment sequences (uses pooled buffers)

### String Handling
- Use `ReadOnlyString` in domain models (Account, Transaction)
- Use `ReadOnlySpan<char>` for parsing operations
- Use `ReadOnlyMemory<char>` for zero-allocation string access
- Avoid `ToString()` calls in hot paths

### Buffer Management
- Use `ArrayPool<T>` for temporary buffers
- Always return rented buffers to the pool
- Use `Utilities.ByteArrayPool` and `Utilities.CharArrayPool` for shared pools

---

## 4. Zero-Allocation Requirements (01-overview.md)

### Core Principle
This is a **zero-allocation financial data I/O library** that uses System.IO.Pipelines for high-performance streaming.

### Zero-Allocation Requirements
- Use `System.IO.Pipelines` for all I/O operations
- Use `ReadOnlySpan<T>` and `ReadOnlyMemory<T>` for zero-allocation access
- Use `ReadOnlyString` for string data in domain models (Account, Transaction)
- Pool buffers using `ArrayPool<T>` when allocations are necessary
- Use `GetSpan()` and `Advance()` pattern for writing to pipes
- Avoid string allocations in hot paths
- **Verify zero-allocation with `MemoryDiagnoser` in benchmarks**

---

## 5. Code Quality Standards (07-code-quality.md)

### DRY Principle
- No duplicate `CreatePipeReader` methods - use `Utilities` helpers
- Single source of truth for PipeReader creation
- Shared utilities for common operations

### Clean Code
- Remove unused imports
- Remove obsolete code
- Remove trailing empty lines
- Keep files concise - split into smaller modules if needed

### Naming Conventions
- **Describe purpose, not implementation**
- **Be specific, not generic**
- **Use domain terminology**
- **Avoid abbreviations**
- **Test names describe behavior** (not implementation details)

### Documentation
- All public APIs must have XML documentation comments
- Comments explain WHY, not WHAT
- Document business rules and non-obvious behavior
- Group related tests with comments

### Compilation
- All code must compile without errors or warnings
- No test failures allowed
- Maintain existing code patterns and conventions

---

## 6. Code Patterns (02-code-patterns.md)

### Creating PipeReader
- Use `Utilities` helpers for zero-allocation creation
- `Utilities.CreatePipeReaderFromString(content)`
- `Utilities.CreatePipeReaderFromBytes(data)`
- `Utilities.CreatePipeReaderFromFileAsync(filePath)`
- `Utilities.CreatePipeReaderFromStreamAsync(stream)`

### Fluent API Pattern
- Entry point: `FinanceReader` static class
- Source builder: `SourceBuilder<T>`
- Format extensions: Provider projects expose `AsCsv()`, `AsOfx()`, etc.
- Configured reader: `ConfiguredReader` combines source + reader

### Domain Models
- Use `readonly struct` for Account and Transaction
- Use `ReadOnlyString` for all string fields
- Provide `FromStrings()` static methods for convenience

---

## 7. Error Handling (05-error-handling.md)

### Source Errors
- File not found: Let `FileNotFoundException` bubble up
- Stream errors: Let `IOException` bubble up
- Invalid data: Return empty enumerable (yield break) or skip invalid records

### Reader Errors
- Invalid format: Skip invalid records, don't throw
- Parse errors: Return empty enumerable or skip record
- Cancellation: Throw `OperationCanceledException`

---

## 8. Extensibility (06-extensibility.md)

### Adding New Sources
1. Create new `*Source` class implementing `ISource`
2. Add `From*()` method to `FinanceReader`
3. Source-specific sources can be async (provide `GetPipeReaderAsync()`)

### Adding New Formats
1. Create new provider project
2. Implement `ISourceReader`
3. Create options class
4. Create extension methods
5. Format automatically works with all sources

---

## Critical Findings: Benchmark Requirements

### Explicit Requirements Found

**From 08-completion.md (line 30)**:
> "**Requirement**: All benchmarks must show **zero allocations** (0 B allocated)"

**From 03-testing.md (line 21)**:
> "All benchmarks must use `[MemoryDiagnoser]` to verify zero-allocation"

**From 01-overview.md (line 27)**:
> "Verify zero-allocation with `MemoryDiagnoser` in benchmarks"

**From 08-completion.md (line 45)**:
> "All benchmarks run without allocations (0 B allocated)"

### Current Benchmark Status

**AllocationBenchmarks**:
- ✅ Most methods: 0 B allocated
- ⚠️ `Account_ToString`: 40 B (implicit string conversion)
- ⚠️ `Transaction_ToString`: 40 B (implicit string conversion)

**CsvParsingBenchmarks**:
- ❌ `ParseAccountsFromCsv`: **2.92 KB** allocated
- ❌ `ParseTransactionsFromCsv`: **3.85 KB** allocated

**OfxParsingBenchmarks**:
- ❌ `ParseAccountsFromOfx`: **2.54 KB** allocated
- ❌ `ParseTransactionsFromOfx`: **3.61 KB** allocated

**PipelineBenchmarks**:
- ❌ `ReadCompleteSequenceAsync`: **4.63 KB** allocated
- ❌ `SequenceToString`: **4.99 KB** allocated

### Analysis

The rules are **explicit and unambiguous**: **All benchmarks must show zero allocations (0 B allocated)**.

However, parsing operations inherently require allocations for:
1. **String allocations** - Parsing text data requires creating string objects
2. **Object creation** - Creating `Account` and `Transaction` objects
3. **Buffer allocations** - Reading from pipelines may allocate buffers

### Recommendation

**Option 1: Strict Compliance** (as per rules)
- Refactor parsing to eliminate all allocations
- Use struct-based models instead of classes
- Avoid string allocations entirely
- This may significantly impact usability

**Option 2: Clarify Requirements** (recommended)
- Zero-allocation applies to **hot paths** and **core utilities** only
- Parsing operations may allocate for object creation
- Separate benchmarks for:
  - Zero-allocation utilities (must be 0 B)
  - Parsing operations (document expected allocations)
- Update rules to clarify scope

**Option 3: Update Rules**
- Change requirement to: "Minimize allocations in hot paths"
- Document expected allocations for parsing operations
- Set allocation budgets per operation type

---

## Current Compliance Status

### ✅ Fully Compliant
- Build: No errors or warnings
- Test Pass Rate: 341/341 (100%)
- Line Coverage: 92.05% (exceeds 90%)
- Code Quality: All standards met
- Code Patterns: All patterns followed
- Error Handling: Guidelines followed
- Diagnostic Code: None present

### ⚠️ Partial Compliance
- **Branch Coverage**: 82.03% (below 90% requirement)
  - Core package: 87.71% (close to 90%)
  - Need to identify and test remaining branches

### ❌ Non-Compliant
- **Benchmark Allocations**: All parsing benchmarks show allocations
  - Rules explicitly require 0 B allocated
  - Current benchmarks: 2.5 KB - 5 KB allocated
  - **Action Required**: Either fix allocations or clarify/update rules

---

## Summary of All Requirements

### Coverage Requirements
- **Line Coverage**: Minimum 90% ✅ (Current: 92.05%)
- **Branch Coverage**: Minimum 90% ⚠️ (Current: 82.03%, Core: 87.71%)
- **Test Pass Rate**: 100% ✅ (Current: 341/341)

### Benchmark Requirements
- **Allocations**: 0 B allocated ❌ (Current: 2.5-5 KB for parsing)
- **MemoryDiagnoser**: Required on all benchmarks ✅
- **API Usage**: Use `FinanceReader` API ✅
- **Data Source**: Use `FromBytes()` for byte arrays ✅

### Code Quality Requirements
- **DRY**: No duplicate code ✅
- **Clean Code**: Remove unused code ✅
- **Naming**: Purpose-driven, domain terminology ✅
- **Documentation**: XML docs on public APIs ✅
- **Compilation**: No errors or warnings ✅

### Performance Requirements
- **Zero-Allocation**: Use `ReadOnlySpan<T>`, `ReadOnlyMemory<T>` ✅
- **Pipelines**: Use `System.IO.Pipelines` ✅
- **Buffers**: Use `ArrayPool<T>` ✅
- **String Handling**: Use `ReadOnlyString` in models ✅

---

## Action Items

### High Priority
1. **Clarify Benchmark Allocation Requirements**
   - Determine if zero-allocation applies to all operations or just utilities
   - Document expected allocations for parsing operations
   - Update rules if needed

2. **Improve Branch Coverage to 90%+**
   - Analyze coverage report for specific uncovered branches
   - Add tests for identified gaps
   - Target: 90% overall (Core is already 87.71%)

### Medium Priority
3. **Review Parsing Allocations**
   - If zero-allocation is required, refactor parsing to eliminate allocations
   - Consider struct-based models
   - Evaluate impact on usability

4. **Separate Benchmark Categories**
   - Zero-allocation utilities (must be 0 B)
   - Parsing operations (document expected allocations)

---

## Conclusion

The rules are comprehensive and mostly clear. The **benchmark allocation requirement is explicit** but may be unrealistic for parsing operations. The project should either:

1. **Achieve strict compliance** by eliminating all allocations (may impact usability)
2. **Clarify the requirement** to specify scope (utilities vs parsing operations)
3. **Update the rules** to reflect realistic allocation expectations

All other requirements are either met or close to being met (branch coverage at 82.03%, Core at 87.71%).



