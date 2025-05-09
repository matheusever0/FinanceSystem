using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.Role;

namespace Equilibrium.Web.Services
{
    public class RoleService : IRoleService
    {
        private readonly IApiService _apiService;

        public RoleService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IEnumerable<RoleModel>> GetAllRolesAsync(string token)
        {
            return await _apiService.GetAsync<IEnumerable<RoleModel>>("/api/Roles", token);
        }

        public async Task<RoleModel> GetRoleByIdAsync(string id, string token)
        {
            return await _apiService.GetAsync<RoleModel>($"/api/Roles/{id}", token);
        }

        public async Task<RoleModel> CreateRoleAsync(CreateRoleModel model, string token)
        {
            return await _apiService.PostAsync<RoleModel>("/api/Roles", model, token);
        }

        public async Task<RoleModel> UpdateRoleAsync(string id, UpdateRoleModel model, string token)
        {
            return await _apiService.PutAsync<RoleModel>($"/api/Roles/{id}", model, token);
        }

        public async Task DeleteRoleAsync(string id, string token)
        {
            await _apiService.DeleteAsync($"/api/Roles/{id}", token);
        }

        public async Task<bool> HasPermissionAsync(string roleId, string permissionSystemName, string token)
        {
            return await _apiService.GetAsync<bool>($"/api/Roles/{roleId}/has-permission/{permissionSystemName}", token);
        }

        public async Task<RoleModel> UpdateRolePermissionsAsync(string roleId, List<string> permissionIds, string token)
        {
            return await _apiService.PutAsync<RoleModel>($"/api/Roles/{roleId}/permissions", permissionIds, token);
        }
        public async Task<PagedResult<RoleModel>> GetFilteredAsync(RoleFilter filter, string token)
        {
            try
            {
                _logger.LogInformation("Obtendo registros filtrados");
                return await _apiService.GetFilteredAsync<PagedResult<RoleModel>>("/api/Roles/filter", filter, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter registros filtrados");
                throw;
            }
        }
