using AutoMapper;
using FinanceSystem.Application.DTOs.Role;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Services;
using FinanceSystem.Resources;

namespace FinanceSystem.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<RoleDto> GetByIdAsync(Guid id)
        {
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(id);
            return role == null ? throw new KeyNotFoundException(ResourceFinanceApi.Role_NotFound) : _mapper.Map<RoleDto>(role);
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _unitOfWork.Roles.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<RoleDto> CreateAsync(CreateRoleDto createRoleDto)
        {
            var existingRole = await _unitOfWork.Roles.GetByNameAsync(createRoleDto.Name);
            if (existingRole != null)
                throw new InvalidOperationException(ResourceFinanceApi.Role_NameExists);

            var role = new Role(createRoleDto.Name, createRoleDto.Description);

            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<RoleDto>(role);
        }

        public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto updateRoleDto)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Role_NotFound);
            if (!string.IsNullOrEmpty(updateRoleDto.Name))
            {
                var existingRole = await _unitOfWork.Roles.GetByNameAsync(updateRoleDto.Name);
                if (existingRole != null && existingRole.Id != id)
                    throw new InvalidOperationException(ResourceFinanceApi.Role_NameExists);

                role.UpdateName(updateRoleDto.Name);
            }

            if (updateRoleDto.Description != null)
            {
                role.UpdateDescription(updateRoleDto.Description);
            }

            await _unitOfWork.Roles.UpdateAsync(role);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<RoleDto>(role);
        }

        public async Task DeleteAsync(Guid id)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Role_NotFound);
            var rolesWithUsers = await _unitOfWork.Roles.GetAllWithUsersAsync();
            var roleWithUsers = rolesWithUsers.FirstOrDefault(r => r?.Id == id);
            if (roleWithUsers != null && roleWithUsers.UserRoles.Count != 0)
                throw new InvalidOperationException(ResourceFinanceApi.Role_HasUsers);

            await _unitOfWork.Roles.DeleteAsync(role);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> HasPermissionAsync(Guid roleId, string permissionSystemName)
        {
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(roleId) ?? throw new KeyNotFoundException(ResourceFinanceApi.Role_NotFound);
            var permission = await _unitOfWork.Permissions.GetBySystemNameAsync(permissionSystemName);
            return permission == null ? false : role.RolePermissions.Any(rp => rp.PermissionId == permission.Id);
        }

        public async Task<RoleDto> UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds)
        {
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(roleId) ?? throw new KeyNotFoundException(ResourceFinanceApi.Role_NotFound);
            var currentPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();

            var permissionsToRemove = currentPermissionIds.Except(permissionIds).ToList();

            var permissionsToAdd = permissionIds.Except(currentPermissionIds).ToList();

            foreach (var permissionId in permissionsToRemove)
            {
                var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
                if (permission != null)
                {
                    role.RemovePermission(permission);
                }
            }

            foreach (var permissionId in permissionsToAdd)
            {
                var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
                if (permission != null)
                {
                    role.AddPermission(permission);
                }
            }

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<RoleDto>(role);
        }
    }
}