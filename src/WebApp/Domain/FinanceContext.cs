using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Domain
{
    public class FinanceContext : DbContext
    {
        public FinanceContext([NotNull] DbContextOptions options) : base(options)
        {
        }

        protected FinanceContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceContext).Assembly);
        }
    }
}