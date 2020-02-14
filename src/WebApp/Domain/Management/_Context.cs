using Microsoft.EntityFrameworkCore;

namespace WebApp.Domain.Management
{
    public class ManagementContext
    {
        private readonly FinanceContext _context;

        public ManagementContext(FinanceContext context) => _context = context ?? throw new System.ArgumentNullException(nameof(context));

        public DbSet<Account> Accounts => _context.Set<Account>();
        public DbSet<AccountType> AccountTypes => _context.Set<AccountType>();
        public DbSet<Category> Categories => _context.Set<Category>();
        public DbSet<Currency> Currencies => _context.Set<Currency>();
    }
}