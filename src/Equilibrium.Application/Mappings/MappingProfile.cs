using AutoMapper;
using Equilibrium.Application.DTOs.CreditCard;
using Equilibrium.Application.DTOs.Financing;
using Equilibrium.Application.DTOs.Income;
using Equilibrium.Application.DTOs.IncomeInstallment;
using Equilibrium.Application.DTOs.IncomeType;
using Equilibrium.Application.DTOs.Payment;
using Equilibrium.Application.DTOs.PaymentInstallment;
using Equilibrium.Application.DTOs.PaymentMethod;
using Equilibrium.Application.DTOs.PaymentType;
using Equilibrium.Application.DTOs.Permission;
using Equilibrium.Application.DTOs.Role;
using Equilibrium.Application.DTOs.User;
using Equilibrium.Domain.Entities;

namespace Equilibrium.Application.Mappings
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