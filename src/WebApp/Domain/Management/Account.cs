using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApp.Domain.Management
{
    public class Account
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string Name { get; set; }
        public int CurrencyId { get; set; }
        public Currency Currency { get; set; }
        public int AccountTypeId { get; set; }
        public AccountType AccountType { get; set; }
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