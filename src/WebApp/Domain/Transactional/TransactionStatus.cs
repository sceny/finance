using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApp.Domain.Transactional
{
    public class TransactionStatus
    {
        public TransactionStatus(TransactionStatusId id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"The {nameof(name)} can not be empty or null.", nameof(name));

            Id = id;
            Name = name;
        }
        public TransactionStatusId Id { get; set; }
        [MaxLength(32)]
        public string Name { get; set; }
    }

    public enum TransactionStatusId
    {
        None = 1,
        Pending = 2,
        Cleared = 3
    }

    internal class TransactionStatusEntityTypeConfiguration : IEntityTypeConfiguration<TransactionStatus>
    {
        public void Configure(EntityTypeBuilder<TransactionStatus> builder)
        {
            builder
                .UseXminAsConcurrencyToken()
                .HasData(
                    new TransactionStatus(TransactionStatusId.None, "None"),
                    new TransactionStatus(TransactionStatusId.Pending, "Pending"),
                    new TransactionStatus(TransactionStatusId.Cleared, "Cleared")
                );
        }
    }
}