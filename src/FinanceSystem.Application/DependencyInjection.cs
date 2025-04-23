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

            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentTypeService, PaymentTypeService>();
            services.AddScoped<IPaymentMethodService, PaymentMethodService>();
            services.AddScoped<ICreditCardService, CreditCardService>();
            services.AddScoped<IPaymentInstallmentService, PaymentInstallmentService>();
            services.AddScoped<IIncomeInstallmentService, IncomeInstallmentService>();
            services.AddScoped<IIncomeService, IncomeService>();
            services.AddScoped<IIncomeTypeService, IncomeTypeService>();
            services.AddScoped<IInvestmentService, InvestmentService>();
            services.AddScoped<IInvestmentTransactionService, InvestmentTransactionService>();
            services.AddScoped<IStockPriceService, StockPriceService>();

            return services;
        }
    }
}