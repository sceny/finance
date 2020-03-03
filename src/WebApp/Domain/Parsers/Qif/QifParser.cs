using System.Collections.Generic;
using System.Threading;

namespace WebApp.Domain.Parsers
{
    // https://en.wikipedia.org/wiki/Quicken_Interchange_Format
    public class QifParser : IParser
    {
        private string content;

        public QifParser(string content)
        {
            this.content = content;
        }

        public static Document Create(
            string content)
            => new Document(new QifParser(content));

        public async IAsyncEnumerable<Account> GetAccountsAsync(CancellationToken cancellationToken)
        {
            var type = content switch
            {
                var c when c.StartsWith("!Type:Cash") => AccountType.Cash,
                var c when c.StartsWith("!Type:Bank") => AccountType.Bank,
                var c when c.StartsWith("!Type:CCard") => AccountType.CreditCard,
                var c when c.StartsWith("!Type:Invst") => AccountType.Investment,
                var c when c.StartsWith("!Type:Oth A") => AccountType.Asset,
                var c when c.StartsWith("!Type:Oth L") => AccountType.Liability,
                var c when c.StartsWith("!Type:Invoice") => AccountType.Invoice,
                _ => AccountType.Unknown
            };
            /*
            
        var c when c.StartsWith("Cash") => AccountType.Cash,
        var c when c.StartsWith("Bank") => AccountType.Bank,
            */
            yield return new Account { Type = type };
        }
    }
}