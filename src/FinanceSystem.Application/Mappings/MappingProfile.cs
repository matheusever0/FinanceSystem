using AutoMapper;
using FinanceSystem.Application.DTOs.CreditCard;
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
        }
    }
}