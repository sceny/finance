using System;
using System.Collections.Generic;
using System.Threading;

namespace WebApp.Domain.Parsers
{
    public interface IParser : IDisposable
    {
        IAsyncEnumerable<Account> GetAccountsAsync(CancellationToken cancellationToken);
        IAsyncEnumerable<AccountItem> GetAccountItemsAsync(CancellationToken cancellationToken);
    }
}