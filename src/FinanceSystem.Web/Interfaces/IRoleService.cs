﻿using FinanceSystem.Web.Models;

namespace FinanceSystem.Web.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleModel>> GetAllRolesAsync(string token);
        Task<RoleModel> GetRoleByIdAsync(string id, string token);
        Task<RoleModel> CreateRoleAsync(CreateRoleModel model, string token);
        Task<RoleModel> UpdateRoleAsync(string id, UpdateRoleModel model, string token);
        Task DeleteRoleAsync(string id, string token);
    }
}