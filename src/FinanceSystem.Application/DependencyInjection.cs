using FinanceSystem.Application.Mappings;
using FinanceSystem.Application.Services;
using FinanceSystem.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceSystem.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Configurar AutoMapper
            services.AddAutoMapper(typeof(MappingProfile));

            // Registrar serviços
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();

            return services;
        }
    }
}
