using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApp.Domain.Management
{
    public class AccountType
    {
        public AccountType(AccountTypeId id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public AccountTypeId Id { get; set; }
        [MaxLength(128)]
        public string Name { get; set; }
    }

    public enum AccountTypeId
    {
        Other = 1,
        Bank = 2,
        Cash = 3,
        CreditCard = 4
    }

    internal class AccountTypeEntityTypeConfiguration : IEntityTypeConfiguration<AccountType>
    {
        public void Configure(EntityTypeBuilder<AccountType> builder)
        {
            builder
                .UseXminAsConcurrencyToken()
                .HasData(
                    new AccountType(AccountTypeId.Other, "Other"),
                    new AccountType(AccountTypeId.Bank, "Bank"),
                    new AccountType(AccountTypeId.Cash, "Cash"),
                    new AccountType(AccountTypeId.CreditCard, "Credit Card")
                );
        }
    }
}