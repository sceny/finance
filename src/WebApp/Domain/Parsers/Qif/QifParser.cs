using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Domain.Parsers.Qif;

namespace WebApp.Domain.Parsers
{
    // https://en.wikipedia.org/wiki/Quicken_Interchange_Format
    public class QifParser : IParser
    {
        public static Document Create(Stream content)
            => new Document(new QifParser(content));

        public static Document Create(string content)
        {
            var byteArray = Encoding.ASCII.GetBytes(content);
            var stream = new MemoryStream(byteArray);
            var parser = new QifParser(stream);
            return new Document(parser);
        }

        private string _line;
        private readonly StreamReader _reader;
        private readonly QifAccountItemFiller _itemFiller;
        private readonly Stream _content;

        public QifParser(Stream input)
        {
            _content = input;
            _reader = new StreamReader(_content);
            _itemFiller = new QifAccountItemFiller();
        }

        private bool IsEndOfRecord => IsEof || _line == "^";
        private bool IsEof => _line == null;

        public async IAsyncEnumerable<Account> GetAccountsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            do
            {
                var account = await ParseAccountAsync();
                if (account == null) // TODO: Move to the end of the items
                    yield break;
                yield return account;
            }
            while (!IsEof);
        }

        public async IAsyncEnumerable<AccountItem> GetAccountItemsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (IsEndOfRecord)
                yield break;
            var item = new AccountItem();
            while (!IsEof)
            {
                _line = await _reader.ReadLineAsync();
                if (IsEof)
                    break;
                if (IsEndOfRecord)
                {
                    yield return item;
                    continue;
                }
                var code = _line.Substring(0, 1);
                var value = _line.Substring(1);
                _itemFiller.Fill(item, code, value);
            };


            // var item = new AccountItem
            // {
            //     Date = new DateTime(2010, 03, 04),
            //     Amount = -379.12,
            //     Payee = "CITY OF SPRINGFIELD"
            // };

            // yield return item;
            yield break;
        }

        private async Task<Account> ParseAccountAsync()
        {
            var isQuickenAccountInfo = false;
            Account? account = null;
            do
            {
                _line = await _reader.ReadLineAsync();
                if (_line == null)
                    return null;

                if (_line.StartsWith("!Type:"))
                {
                    GetAccount().Type = GetType(_line.Substring(6));
                    break;
                }

                if (_line == "!Account")
                    isQuickenAccountInfo = true;

                if (isQuickenAccountInfo)
                {
                    if (_line.StartsWith("N"))
                        GetAccount().Name = _line.Substring(1);
                    else if (_line.StartsWith("T"))
                        GetAccount().Type = GetType(_line.Substring(1));
                    else if (_line.StartsWith("D"))
                        GetAccount().Description = _line.Substring(1);
                }

                if (IsEndOfRecord)
                {
                    isQuickenAccountInfo = false;
                    continue;
                }
            }
            while (!IsEof);
            return account;

            Account GetAccount() => account ??= new Account(this);
        }

        private static AccountType GetType(string value) => value switch
        {
            "Cash" => AccountType.Cash,
            "Bank" => AccountType.Bank,
            "CCard" => AccountType.CreditCard,
            "Invst" => AccountType.Investment,
            "Oth A" => AccountType.Asset,
            "Oth L" => AccountType.Liability,
            "Invoice" => AccountType.Invoice,
            _ => AccountType.Unknown
        };

        public void Dispose() => _reader?.Dispose();
    }
}