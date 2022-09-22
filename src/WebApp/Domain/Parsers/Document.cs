using System.Collections.Generic;
using System.Threading;

namespace WebApp.Domain.Parsers;

public class Document
{
    private readonly IParser _parser;
    public Document(IParser parser)
        => _parser = parser ?? throw new System.ArgumentNullException(nameof(parser));

    public IAsyncEnumerable<Account> GetAccountsAsync(
        CancellationToken cancellationToken = default)
        => _parser.GetAccountsAsync(cancellationToken);
}