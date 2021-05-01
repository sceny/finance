using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Domain.Transactional;

namespace WebApp.Domain.Management
{
    public class Category
    {
        private TransactionKind? _kind;

        public int Id { get; set; }
        [MaxLength(128)]
        public string Name { get; set; } = "";
        public TransactionKindId KindId { get; set; }
        public TransactionKind Kind
        {
            get => Check.EFLoaded(_kind);
            set => _kind = value;
        }
    }

    internal class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.UseXminAsConcurrencyToken();
        }
    }
}