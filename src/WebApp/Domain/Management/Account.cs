using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApp.Domain.Management
{
    public class Account
    {
        private Currency? _currency;
        private AccountType? _accountType;

        public int Id { get; set; }
        [MaxLength(128)]
        public string Name { get; set; } = "";
        public int CurrencyId { get; set; }
        public Currency Currency
        {
            get => Check.EFLoaded(_currency);
            set => _currency = value;
        }
        public int AccountTypeId { get; set; }
        public AccountType AccountType
        {
            get => Check.EFLoaded(_accountType);
            set => _accountType = value;
        }
        [MaxLength(128)]
        public string? Institution { get; set; }
        [MaxLength(128)]
        public string? Number { get; set; }
    }

    internal class AccountEntityTypeConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.UseXminAsConcurrencyToken();
        }
    }
}