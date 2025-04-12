using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models;

namespace FinanceSystem.Web.Services
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
            return await _apiService.GetAsync<IEnumerable<RoleModel>>("/api/roles", token);
        }

        public async Task<RoleModel> GetRoleByIdAsync(string id, string token)
        {
            return await _apiService.GetAsync<RoleModel>($"/api/roles/{id}", token);
        }

        public async Task<RoleModel> CreateRoleAsync(CreateRoleModel model, string token)
        {
            return await _apiService.PostAsync<RoleModel>("/api/roles", model, token);
        }

        public async Task<RoleModel> UpdateRoleAsync(string id, UpdateRoleModel model, string token)
        {
            return await _apiService.PutAsync<RoleModel>($"/api/roles/{id}", model, token);
        }

        public async Task DeleteRoleAsync(string id, string token)
        {
            await _apiService.DeleteAsync($"/api/roles/{id}", token);
        }
    }
}