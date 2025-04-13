using FinanceSystem.Application.Interfaces;
using FinanceSystem.Application.Mappings;
using FinanceSystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceSystem.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();

            return services;
        }
    }
}