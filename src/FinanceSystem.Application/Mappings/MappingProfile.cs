using AutoMapper;
using FinanceSystem.Application.DTOs.CreditCard;
using FinanceSystem.Application.DTOs.Financing;
using FinanceSystem.Application.DTOs.Income;
using FinanceSystem.Application.DTOs.IncomeInstallment;
using FinanceSystem.Application.DTOs.IncomeType;
using FinanceSystem.Application.DTOs.Investment;
using FinanceSystem.Application.DTOs.InvestmentTransaction;
using FinanceSystem.Application.DTOs.Payment;
using FinanceSystem.Application.DTOs.PaymentInstallment;
using FinanceSystem.Application.DTOs.PaymentMethod;
using FinanceSystem.Application.DTOs.PaymentType;
using FinanceSystem.Application.DTOs.Permission;
using FinanceSystem.Application.DTOs.Role;
using FinanceSystem.Application.DTOs.User;
using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()));

            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Permissions,
                opt => opt.MapFrom(src => src.RolePermissions.Select(rp => rp.Permission.SystemName).ToList()));

            CreateMap<Permission, PermissionDto>();

            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.PaymentTypeName, opt => opt.MapFrom(src => src.PaymentType.Name))
                .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.Name));

            CreateMap<PaymentType, PaymentTypeDto>();

            CreateMap<PaymentMethod, PaymentMethodDto>();

            CreateMap<CreditCard, CreditCardDto>()
                .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src => src.PaymentMethod.Name));

            CreateMap<PaymentInstallment, PaymentInstallmentDto>();

            CreateMap<Income, IncomeDto>()
                .ForMember(dest => dest.IncomeTypeName,
                opt => opt.MapFrom(src => src.IncomeType.Name));

            CreateMap<IncomeType, IncomeTypeDto>();

            CreateMap<IncomeInstallment, IncomeInstallmentDto>();

            CreateMap<Investment, InvestmentDto>()
                .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src => src.Transactions))
                .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<InvestmentTransaction, InvestmentTransactionDto>()
                .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.Type.ToString()));

            CreateMap<Financing, FinancingDto>()
                .ForMember(dest => dest.TotalPaid, opt => opt.Ignore())
                .ForMember(dest => dest.TotalRemaining, opt => opt.Ignore())
                .ForMember(dest => dest.ProgressPercentage, opt => opt.Ignore())
                .ForMember(dest => dest.InstallmentsPaid, opt => opt.Ignore())
                .ForMember(dest => dest.InstallmentsRemaining, opt => opt.Ignore());

            CreateMap<Financing, FinancingDetailDto>()
                .ForMember(dest => dest.TotalPaid, opt => opt.Ignore())
                .ForMember(dest => dest.TotalRemaining, opt => opt.Ignore())
                .ForMember(dest => dest.ProgressPercentage, opt => opt.Ignore())
                .ForMember(dest => dest.InstallmentsPaid, opt => opt.Ignore())
                .ForMember(dest => dest.InstallmentsRemaining, opt => opt.Ignore())
                .ForMember(dest => dest.TotalInterestPaid, opt => opt.Ignore())
                .ForMember(dest => dest.TotalInterestRemaining, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmortizationPaid, opt => opt.Ignore())
                .ForMember(dest => dest.AverageInstallmentAmount, opt => opt.Ignore())
                .ForMember(dest => dest.MonthlyAveragePayment, opt => opt.Ignore())
                .ForMember(dest => dest.EstimatedTotalCost, opt => opt.Ignore())
                .ForMember(dest => dest.Installments, opt => opt.MapFrom(src => src.Installments))
                .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments));

            CreateMap<FinancingInstallment, FinancingInstallmentDto>();
            CreateMap<FinancingInstallment, FinancingInstallmentDetailDto>()
                .ForMember(dest => dest.FinancingDescription, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());
        }
    }
}