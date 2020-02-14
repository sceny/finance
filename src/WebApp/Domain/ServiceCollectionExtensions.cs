using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Domain.Budget;
using WebApp.Domain.Management;
using WebApp.Domain.Transactional;

namespace WebApp.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<FinanceContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("FinanceContext");
                options
                    .UseNpgsql(
                        connectionString,
                        options => options.EnableRetryOnFailure())
                    .UseSnakeCaseNamingConvention();
            });
            services.AddTransient<BudgetContext>();
            services.AddTransient<ManagementContext>();
            services.AddTransient<TransactionalContext>();
            return services;
        }
    }
}