﻿using AutoMapper;
using FinanceSystem.Application.DTOs;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Services;

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
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {id} not found");

            return _mapper.Map<RoleDto>(role);
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _unitOfWork.Roles.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<RoleDto> CreateAsync(CreateRoleDto createRoleDto)
        {
            // Verificar se já existe uma role com esse nome
            var existingRole = await _unitOfWork.Roles.GetByNameAsync(createRoleDto.Name);
            if (existingRole != null)
                throw new InvalidOperationException($"Role '{createRoleDto.Name}' already exists");

            // Criar role
            var role = new Role(createRoleDto.Name, createRoleDto.Description);

            // Persistir no banco de dados
            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<RoleDto>(role);
        }

        public async Task<RoleDto> UpdateAsync(Guid id, UpdateRoleDto updateRoleDto)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {id} not found");

            // Atualizar nome se fornecido
            if (!string.IsNullOrEmpty(updateRoleDto.Name))
            {
                var existingRole = await _unitOfWork.Roles.GetByNameAsync(updateRoleDto.Name);
                if (existingRole != null && existingRole.Id != id)
                    throw new InvalidOperationException($"Role '{updateRoleDto.Name}' already exists");

                role.UpdateName(updateRoleDto.Name);
            }

            // Atualizar descrição se fornecida
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
            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {id} not found");

            // Verificar se existem usuários com essa role
            var rolesWithUsers = await _unitOfWork.Roles.GetAllWithUsersAsync();
            var roleWithUsers = rolesWithUsers.FirstOrDefault(r => r.Id == id);
            if (roleWithUsers != null && roleWithUsers.UserRoles.Any())
                throw new InvalidOperationException("Cannot delete role because it is assigned to users");

            await _unitOfWork.Roles.DeleteAsync(role);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> HasPermissionAsync(Guid roleId, string permissionSystemName)
        {
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(roleId);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {roleId} not found");

            var permission = await _unitOfWork.Permissions.GetBySystemNameAsync(permissionSystemName);
            if (permission == null)
                return false;

            return role.RolePermissions.Any(rp => rp.PermissionId == permission.Id);
        }

        public async Task<RoleDto> UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds)
        {
            var role = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(roleId);
            if (role == null)
                throw new KeyNotFoundException($"Role with ID {roleId} not found");

            // Obter todas as permissões atuais do papel
            var currentPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToList();

            // Permissões a serem removidas (estão em currentPermissionIds, mas não em permissionIds)
            var permissionsToRemove = currentPermissionIds.Except(permissionIds).ToList();

            // Permissões a serem adicionadas (estão em permissionIds, mas não em currentPermissionIds)
            var permissionsToAdd = permissionIds.Except(currentPermissionIds).ToList();

            // Remover permissões
            foreach (var permissionId in permissionsToRemove)
            {
                var permission = await _unitOfWork.Permissions.GetByIdAsync(permissionId);
                if (permission != null)
                {
                    role.RemovePermission(permission);
                }
            }

            // Adicionar permissões
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