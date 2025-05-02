using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Infrastructure.Data;
using Equilibrium.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Equilibrium.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>
                (options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("Equilibrium.Infrastructure")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<IPermissionAuthorizationService, PermissionAuthorizationService>();
            services.AddScoped<IRecurrenceService, RecurrenceService>();
            services.AddHostedService<RecurrenceProcessorService>();
            services.AddHostedService<DatabaseInitializer>();

            services.AddHttpClient();

            return services;
        }
    }
}