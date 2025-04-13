using AutoMapper;
using FinanceSystem.Application.DTOs;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Services;

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
            if (permission == null)
                throw new KeyNotFoundException($"Permission with ID {id} not found");

            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task<IEnumerable<PermissionDto>> GetAllAsync()
        {
            var permissions = await _unitOfWork.Permissions.GetAllAsync();
            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public async Task<PermissionDto> CreateAsync(CreatePermissionDto createPermissionDto)
        {
            // Verificar se já existe uma permissão com o mesmo SystemName
            var existingPermission = await _unitOfWork.Permissions.GetBySystemNameAsync(createPermissionDto.SystemName);
            if (existingPermission != null)
                throw new InvalidOperationException($"Permission with SystemName '{createPermissionDto.SystemName}' already exists");

            // Criar permissão
            var permission = new Permission(
                createPermissionDto.Name,
                createPermissionDto.SystemName,
                createPermissionDto.Description);

            // Persistir no banco de dados
            await _unitOfWork.Permissions.AddAsync(permission);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<PermissionDto>(permission);
        }

        public async Task<PermissionDto> UpdateAsync(Guid id, UpdatePermissionDto updatePermissionDto)
        {
            var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
            if (permission == null)
                throw new KeyNotFoundException($"Permission with ID {id} not found");

            // Atualizar nome se fornecido
            if (!string.IsNullOrEmpty(updatePermissionDto.Name))
            {
                permission.UpdateName(updatePermissionDto.Name);
            }

            // Atualizar descrição se fornecida
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
            var permission = await _unitOfWork.Permissions.GetByIdAsync(id);
            if (permission == null)
                throw new KeyNotFoundException($"Permission with ID {id} not found");

            await _unitOfWork.Permissions.DeleteAsync(permission);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(Guid roleId)
        {
            // Verificar se a role existe
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {roleId} not found");

            var permissions = await _unitOfWork.Permissions.GetPermissionsByRoleIdAsync(roleId);
            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }

        public async Task<bool> AssignPermissionToRoleAsync(Guid roleId, Guid permissionId)
        {
            // Verificar se a role existe
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(roleId);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {roleId} not found");

            // Verificar se a permissão existe
            var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
            if (permission == null)
                throw new KeyNotFoundException($"Permission with ID {permissionId} not found");

            // Verificar se a role já possui a permissão
            if (role.RolePermissions.Any(rp => rp.PermissionId == permissionId))
                return false; // Permissão já atribuída

            // Adicionar permissão à role
            role.AddPermission(permission);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
        {
            // Verificar se a role existe
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(roleId);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {roleId} not found");

            // Verificar se a permissão existe
            var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
            if (permission == null)
                throw new KeyNotFoundException($"Permission with ID {permissionId} not found");

            // Verificar se a role possui a permissão
            if (!role.RolePermissions.Any(rp => rp.PermissionId == permissionId))
                return false; // Role não possui a permissão

            // Remover permissão da role
            role.RemovePermission(permission);

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByUserIdAsync(Guid userId)
        {
            // Verificar se o usuário existe
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            // Obter todas as roles do usuário
            var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

            // Obter todas as permissões de todas as roles do usuário
            var permissionDtos = new List<PermissionDto>();
            foreach (var roleId in roleIds)
            {
                var permissions = await _unitOfWork.Permissions.GetPermissionsByRoleIdAsync(roleId);
                permissionDtos.AddRange(_mapper.Map<IEnumerable<PermissionDto>>(permissions));
            }

            // Remover duplicatas (um usuário pode ter a mesma permissão de diferentes roles)
            return permissionDtos.GroupBy(p => p.Id).Select(g => g.First());
        }
    }
}