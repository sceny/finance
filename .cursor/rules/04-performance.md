# Performance Guidelines

## System.IO.Pipelines Usage
- Always use `Utilities.ReadCompleteSequenceAsync()` to read complete sequences
- Always call `Utilities.AdvanceReader()` after consuming data
- Use `ReadOnlySequence<byte>` for zero-allocation access
- Use `SequenceToString()` for multi-segment sequences (uses pooled buffers)

## String Handling
- Use `ReadOnlyString` in domain models (Account, Transaction)
- Use `ReadOnlySpan<char>` for parsing operations
- Use `ReadOnlyMemory<char>` for zero-allocation string access
- Avoid `ToString()` calls in hot paths

## Buffer Management
- Use `ArrayPool<T>` for temporary buffers
- Always return rented buffers to the pool
- Use `Utilities.ByteArrayPool` and `Utilities.CharArrayPool` for shared pools

