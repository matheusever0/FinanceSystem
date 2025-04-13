using FinanceSystem.Domain.Interfaces.Services;
using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Services;

namespace FinanceSystem.Web.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();

            return services;
        }
    }
}