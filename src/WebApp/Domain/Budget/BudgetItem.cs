using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Domain.Management;

namespace WebApp.Domain.Budget
{
    public class BudgetItem
    {
        public int Id { get; set; }
        public DateTime Month { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }

    internal class BudgetItemEntityTypeConfiguration : IEntityTypeConfiguration<BudgetItem>
    {
        public void Configure(EntityTypeBuilder<BudgetItem> builder)
        {
            builder.UseXminAsConcurrencyToken();
        }
    }
}