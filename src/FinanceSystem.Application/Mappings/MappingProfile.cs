using AutoMapper;
using FinanceSystem.Application.DTOs;
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
        }
    }
}