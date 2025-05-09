using Equilibrium.Application.Interfaces;
using Equilibrium.Application.Mappings;
using Equilibrium.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

namespace Equilibrium.Application
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
            services.AddScoped<IFinancingService, FinancingService>();
            services.AddScoped<IFinancingInstallmentService, FinancingInstallmentService>();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
