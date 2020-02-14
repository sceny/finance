using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Domain.Transactional;

namespace WebApp.Domain.Management
{
    public class Category
    {
        public int Id { get; set; }
        [MaxLength(128)]
        public string Name { get; set; }
        public TransactionKindId KindId { get; set; }
        public TransactionKind Kind { get; set; }
    }

    internal class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.UseXminAsConcurrencyToken();
        }
    }
}