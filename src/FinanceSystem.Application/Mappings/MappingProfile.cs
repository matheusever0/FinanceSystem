using AutoMapper;
using FinanceSystem.Application.DTOs.CreditCard;
using FinanceSystem.Application.DTOs.Payment;
using FinanceSystem.Application.DTOs.PaymentInstallmentDto;
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
        }
    }
}