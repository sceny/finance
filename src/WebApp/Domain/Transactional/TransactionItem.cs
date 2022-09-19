using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApp.Domain.Management;

namespace WebApp.Domain.Transactional;

public class TransactionItem
{
    private Account? _fromAccount;
    private Category? _category;
    private TransactionKind? _kind;
    private TransactionStatus? _status;

    public int Id { get; set; }
    public DateTime Date { get; set; }
    public double Amount { get; set; }
    [MaxLength(2000)]
    public string Memo { get; set; } = "";
    [MaxLength(2000)]
    public string Information { get; set; } = "";
    public int FromAccountId { get; set; }
    public Account FromAccount
    {
        get => Check.EFLoaded(_fromAccount);
        set => _fromAccount = value;
    }
    public int? ToAccountId { get; set; }
    public Account? ToAccount { get; set; }
    public int CategoryId { get; set; }
    public Category Category
    {
        get => Check.EFLoaded(_category);
        set => _category = value;
    }
    public TransactionKindId KindId { get; set; }
    public TransactionKind Kind
    {
        get => Check.EFLoaded(_kind);
        set => _kind = value;
    }
    public TransactionStatusId StatusId { get; set; }
    public TransactionStatus Status
    {
        get => Check.EFLoaded(_status);
        set => _status = value;
    }
}

internal class TransactionItemEntityTypeConfiguration : IEntityTypeConfiguration<TransactionItem>
{
    public void Configure(EntityTypeBuilder<TransactionItem> builder)
    {
        builder.UseXminAsConcurrencyToken();
    }
}