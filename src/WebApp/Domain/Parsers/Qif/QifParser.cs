using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly Stream _content;

        public QifParser(Stream input)
        {
            _content = input;
            _reader = new StreamReader(_content);
        }

        private bool IsEndOfRecord => IsEndOfFile || _line == "^";
        private bool IsEndOfFile => _line == null;

        public async IAsyncEnumerable<Account> GetAccountsAsync(
            [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            Account account = null;
            do
            {
                if (account == null)
                {
                    account = await TryParseHeaderAsync();
                    if (account == null)
                        yield break;
                    if (IsEndOfRecord)
                    {
                        yield return account;
                        account = null;
                        continue;
                    }
                }
                else
                {
                    _line = await _reader.ReadLineAsync();
                }

                if (IsEndOfRecord)
                {
                    yield return account;
                    account = null;
                }
            }
            while (_line != null);
        }

        public async IAsyncEnumerable<AccountItem> GetAccountItemsAsync(
            [EnumeratorCancellation]CancellationToken cancellationToken)
        {
            do
            {
            }
            while (!IsEndOfFile);


            // var item = new AccountItem
            // {
            //     Date = new DateTime(2010, 03, 04),
            //     Amount = -379.12,
            //     Payee = "CITY OF SPRINGFIELD"
            // };

            // yield return item;
            yield break;
        }

        private async Task<Account> TryParseHeaderAsync()
        {
            var isQuickenMode = false;
            var account = new Account(this);
            do
            {
                _line = await _reader.ReadLineAsync();
                if (_line == null)
                    return null;

                if (_line == "!Account")
                    isQuickenMode = true;

                if (isQuickenMode)
                {
                    if (_line.StartsWith("N"))
                        account.Name = _line.Substring(1);
                    if (_line.StartsWith("T"))
                        account.Type = GetType(_line.Substring(1));
                    if (_line.StartsWith("D"))
                        account.Description = _line.Substring(1);
                }

                if (IsEndOfRecord)
                {
                    if (!isQuickenMode)
                        break;
                    isQuickenMode = false;
                    continue;
                }

                if (_line.StartsWith("!Type:"))
                    account.Type = GetType(_line.Substring(6));
            }
            while (_line != null);
            return account;
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