using Microsoft.EntityFrameworkCore;

namespace WebApp.Domain.Budget;

public class BudgetContext
{
    private readonly FinanceContext _context;

    public BudgetContext(FinanceContext context) => _context = context ?? throw new System.ArgumentNullException(nameof(context));

    public DbSet<BudgetItem> Items => _context.Set<BudgetItem>();
}