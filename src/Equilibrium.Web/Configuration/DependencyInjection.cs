using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Services;

namespace Equilibrium.Web.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {

            services.AddScoped<IApiService, ApiService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IWebPermissionAuthorizationService, WebPermissionAuthorizationService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentTypeService, PaymentTypeService>();
            services.AddScoped<IPaymentMethodService, PaymentMethodService>();
            services.AddScoped<ICreditCardService, CreditCardService>();
            services.AddScoped<IIncomeService, IncomeService>();
            services.AddScoped<IIncomeTypeService, IncomeTypeService>();
            services.AddScoped<IFinancingService, FinancingService>();
            services.AddScoped<ITokenManagerService, TokenManagerService>();

            return services;
        }
    }
}