using System.Collections.Generic;
using System.Threading;

namespace WebApp.Domain.Parsers
{
    public interface IParser
    {
        IAsyncEnumerable<Account> GetAccountsAsync(CancellationToken cancellationToken);
    }
}