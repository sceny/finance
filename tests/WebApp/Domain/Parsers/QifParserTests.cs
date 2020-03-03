using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using WebApp.Domain.Parsers;
using Xunit;

namespace Sceny.Finance.WebApp.Tests.Domain.Parsers
{
    public class QifParserTests
    {

        [Theory]
        [InlineData("Cash", AccountType.Cash)]
        [InlineData("Bank", AccountType.Bank)]
        [InlineData("CCard", AccountType.CreditCard)]
        [InlineData("Invst", AccountType.Investment)]
        [InlineData("Oth A", AccountType.Asset)]
        [InlineData("Oth L", AccountType.Liability)]
        [InlineData("Invoice", AccountType.Invoice)]
        public async Task Account_types(string qifType, AccountType accountType)
        {
            // arrange
            var bankRecord = $@"!Type:{qifType}
^";
            // act
            var doc = QifParser.Create(bankRecord);

            // assert
            var accounts = await doc.GetAccounts().ToArrayAsync();
            accounts.Should().HaveCount(1);
            accounts[0].Should().NotBeNull();
            accounts[0].Type.Should().Be(accountType);
        }

    }
}