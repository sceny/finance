using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApp.Domain.Management;

public class Currency
{
    public Currency(int id, string name)
    {
        Id = id;
        Name = name ?? throw new System.ArgumentNullException(nameof(name));
    }
    public int Id { get; set; }
    [MaxLength(128)]
    public string Name { get; set; }
}

internal class CurrencyEntityTypeConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.UseXminAsConcurrencyToken();
    }
}