// src/FinanceSystem.Infrastructure/DependencyInjection.cs
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
            // Configurar SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("FinanceSystem.Infrastructure")
                )
            );

            // Registrar serviços
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDbInitializer, DbInitializer>();

            // Registrar o inicializador de banco de dados
            services.AddHostedService<DatabaseInitializer>();

            return services;
        }
    }
}