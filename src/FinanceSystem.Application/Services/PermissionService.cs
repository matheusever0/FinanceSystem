using AutoMapper;
using FinanceSystem.Application.DTOs.Permission;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Services;
using FinanceSystem.Resources;

namespace FinanceSystem.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PermissionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PermissionDto> GetByIdAsync(Guid id)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
            return permission == null
                ? throw new KeyNotFoundException(ResourceFinanceApi.Payment_NotFound)
                : _mapper.Map<PermissionDto>(permission);
        }

        public async Task<IEnumerable<PermissionDto>> GetAllAsync()
        {
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public async Task<PermissionDto> CreateAsync(CreatePermissionDto createPermissionDto)
        {
            var existingPermission = await _unitOfWork.Permissions.GetBySystemNameAsync(createPermissionDto.SystemName);
            if (existingPermission != null)
                throw new InvalidOperationException(ResourceFinanceApi.Permission_SystemNameExists);

            var permission = new Permission(createPermissionDto.Name, createPermissionDto.SystemName, createPermissionDto.Description);

            await _unitOfWork.Permissions.AddAsync(permission);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task<PermissionDto> UpdateAsync(Guid id, UpdatePermissionDto updatePermissionDto)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Permission_NotFound);
            if (!string.IsNullOrEmpty(updatePermissionDto.Name))
            {
                permission.UpdateName(updatePermissionDto.Name);
            }

            if (updatePermissionDto.Description != null)
            {
                permission.UpdateDescription(updatePermissionDto.Description);
            }

            await _unitOfWork.Permissions.UpdateAsync(permission);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task DeleteAsync(Guid id)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Permission_NotFound);
            await _unitOfWork.Permissions.DeleteAsync(permission);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(Guid roleId)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId) ?? throw new KeyNotFoundException(ResourceFinanceApi.Role_NotFound);
            var permissions = await _unitOfWork.Permissions.GetPermissionsByRoleIdAsync(roleId);
            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId)
        {
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(roleId) ?? throw new KeyNotFoundException(ResourceFinanceApi.Role_NotFound);
            var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId) ?? throw new KeyNotFoundException(ResourceFinanceApi.Permission_NotFound);
            if (role.RolePermissions.Any(rp => rp.PermissionId == permissionId))
                return false;
            role.AddPermission(permission);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
        {
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(roleId) ?? throw new KeyNotFoundException(ResourceFinanceApi.Role_NotFound);
            var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId) ?? throw new KeyNotFoundException(ResourceFinanceApi.Permission_NotFound);
            if (!role.RolePermissions.Any(rp => rp.PermissionId == permissionId))
                return false;
            role.RemovePermission(permission);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByUserIdAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(userId) ?? throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound);
            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

            var permissionDtos = new List<PermissionDto>();
            foreach (var roleId in roleIds)
            {
                var permissions = await _unitOfWork.Permissions.GetPermissionsByRoleIdAsync(roleId);
                permissionDtos.AddRange(_mapper.Map<IEnumerable<PermissionDto>>(permissions));
            }

            return permissionDtos.GroupBy(p => p.Id).Select(g => g.First());
        }
    }
}