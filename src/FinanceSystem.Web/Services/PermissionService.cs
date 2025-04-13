using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models;

namespace FinanceSystem.Web.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IApiService _apiService;

        public PermissionService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IEnumerable<PermissionModel>> GetAllPermissionsAsync(string token)
        {
            return await _apiService.GetAsync<IEnumerable<PermissionModel>>("/api/permissions", token);
        }

        public async Task<PermissionModel> GetPermissionByIdAsync(string id, string token)
        {
            return await _apiService.GetAsync<PermissionModel>($"/api/permissions/{id}", token);
        }

        public async Task<PermissionModel> CreatePermissionAsync(CreatePermissionModel model, string token)
        {
            return await _apiService.PostAsync<PermissionModel>("/api/permissions", model, token);
        }

        public async Task<PermissionModel> UpdatePermissionAsync(string id, UpdatePermissionModel model, string token)
        {
            return await _apiService.PutAsync<PermissionModel>($"/api/permissions/{id}", model, token);
        }

        public async Task DeletePermissionAsync(string id, string token)
        {
            await _apiService.DeleteAsync($"/api/permissions/{id}", token);
        }

        public async Task<IEnumerable<PermissionModel>> GetPermissionsByRoleIdAsync(string roleId, string token)
        {
            return await _apiService.GetAsync<IEnumerable<PermissionModel>>($"/api/permissions/role/{roleId}", token);
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId, string token)
        {
            return await _apiService.PostAsync<bool>($"/api/permissions/role/{roleId}/permission/{permissionId}", null, token);
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId, string token)
        {
            return await _apiService.DeleteAsync<bool>($"/api/permissions/role/{roleId}/permission/{permissionId}", token);
        }

        public async Task<IEnumerable<PermissionModel>> GetPermissionsByUserIdAsync(string userId, string token)
        {
            return await _apiService.GetAsync<IEnumerable<PermissionModel>>($"/api/permissions/user/{userId}", token);
        }
    }
}