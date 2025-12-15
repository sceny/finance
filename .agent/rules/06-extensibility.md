---
trigger: always
---

# Future Extensibility

## Adding New Sources
1. Create new `*Source` class implementing `ISource`
2. Add `From*()` method to `FinanceReader`
3. Source-specific sources can be async (provide `GetPipeReaderAsync()`)

## Adding New Formats
1. Create new provider project (e.g., `Sceny.Finance.IO.Http`)
2. Implement `ISourceReader` (e.g., `JsonSourceReader`)
3. Create options class (e.g., `JsonOptions`)
4. Create extension methods (e.g., `AsJson()`)
5. Format automatically works with all sources

## Future Sources (Designed But Not Implemented)
- HttpSource: HTTP endpoint source
- SignalRSource: SignalR connection source
- MessageBusSource: Message bus subscription source
