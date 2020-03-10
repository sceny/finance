using System;
using System.Collections.Generic;
using System.Threading;

namespace WebApp.Domain.Parsers
{
    public class Account
    {
        private readonly IParser _parser;
        public Account(IParser parser)
            => _parser = parser ?? throw new ArgumentNullException(nameof(parser));

        public AccountType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public IAsyncEnumerable<AccountItem> GetItems(
            CancellationToken cancellationToken = default)
            => _parser.GetAccountItemsAsync(cancellationToken);
    }
}