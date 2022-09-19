using Microsoft.EntityFrameworkCore;

namespace WebApp.Domain.Transactional;

public class TransactionalContext
{
    private readonly FinanceContext _context;

    public TransactionalContext(FinanceContext context) => _context = context ?? throw new System.ArgumentNullException(nameof(context));

    public DbSet<TransactionItem> Items => _context.Set<TransactionItem>();
    public DbSet<TransactionKind> Kinds => _context.Set<TransactionKind>();
    public DbSet<TransactionStatus> Statuses => _context.Set<TransactionStatus>();
}