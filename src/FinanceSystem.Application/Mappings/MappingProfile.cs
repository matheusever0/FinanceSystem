using AutoMapper;
using FinanceSystem.Application.DTOs;
using FinanceSystem.Domain.Entities;

namespace FinanceSystem.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User -> UserDto
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                    src.UserRoles.Select(ur => ur.Role.Name).ToList()));

            // Role -> RoleDto
            CreateMap<Role, RoleDto>();
        }
    }
}
