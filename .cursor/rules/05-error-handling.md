# Error Handling

## Source Errors
- File not found: Let FileNotFoundException bubble up
- Stream errors: Let IOException bubble up
- Invalid data: Return empty enumerable (yield break) or skip invalid records

## Reader Errors
- Invalid format: Skip invalid records, don't throw
- Parse errors: Return empty enumerable or skip record
- Cancellation: Throw `OperationCanceledException`

