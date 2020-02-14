using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Domain.Management;

namespace WebApp.Domain.Transactional
{
    public class TransactionItem
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        [MaxLength(2000)]
        public string Memo { get; set; } = "";
        [MaxLength(2000)]
        public string Information { get; set; } = "";
        public int FromAccountId { get; set; }
        public Account FromAccount { get; set; }
        public int? ToAccountId { get; set; }
        public Account? ToAccount { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public TransactionKindId KindId { get; set; }
        public TransactionKind Kind { get; set; }
        public TransactionStatusId StatusId { get; set; }
        public TransactionStatus Status { get; set; }
    }

    internal class TransactionItemEntityTypeConfiguration : IEntityTypeConfiguration<TransactionItem>
    {
        public void Configure(EntityTypeBuilder<TransactionItem> builder)
        {
            builder.UseXminAsConcurrencyToken();
        }
    }
}