using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApp.Domain.Transactional
{
    public class TransactionKind
    {
        public TransactionKind(TransactionKindId id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        public TransactionKindId Id { get; set; }
        [MaxLength(32)]
        public string Name { get; set; }
    }

    public enum TransactionKindId
    {
        Income = 1,
        Expense = 2,
        Transfer = 3
    }

    internal class TransactionKindEntityTypeConfiguration : IEntityTypeConfiguration<TransactionKind>
    {
        public void Configure(EntityTypeBuilder<TransactionKind> builder)
        {
            builder
                .UseXminAsConcurrencyToken()
                .HasData(
                    new TransactionKind(TransactionKindId.Income, "Income"),
                    new TransactionKind(TransactionKindId.Expense, "Expense"),
                    new TransactionKind(TransactionKindId.Transfer, "Transfer")
                );
        }
    }
}