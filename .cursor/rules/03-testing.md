# Testing Requirements

## Test Coverage
- **Minimum 90% code coverage** for all new code
- **All tests MUST pass** - no exceptions
- Test both success and error scenarios
- Test all configuration options
- Test cancellation tokens
- Test edge cases (empty files, invalid data, etc.)

## Test Patterns
- Use `FinanceReader` API in tests (not direct ISourceReader usage)
- Remove duplicate `CreatePipeReader` methods - use `Utilities.CreatePipeReaderFromString()` or `FinanceReader.FromString()`
- Test both default and configured options
- Use `ToListAsync()` for assertions in tests

## Benchmark Requirements
- All benchmarks must use `[MemoryDiagnoser]` to verify zero-allocation
- Benchmarks should use `FinanceReader` API
- Use `FromBytes()` for byte array data in benchmarks

