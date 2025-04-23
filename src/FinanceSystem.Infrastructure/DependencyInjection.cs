using FinanceSystem.Domain.Interfaces.Services;
using FinanceSystem.Infrastructure.Data;
using FinanceSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("FinanceSystem.Infrastructure")
    )
);

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<IPermissionAuthorizationService, PermissionAuthorizationService>();

            services.AddHostedService<DatabaseInitializer>();

            services.AddHttpClient();

            return services;
        }
    }
}