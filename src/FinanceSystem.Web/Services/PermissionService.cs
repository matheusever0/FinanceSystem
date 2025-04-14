using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models;
using System.Collections.Concurrent;

namespace FinanceSystem.Web.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<PermissionService> _logger;
        private readonly ConcurrentDictionary<string, (DateTime Timestamp, IEnumerable<PermissionModel> Permissions)> _userPermissionsCache;

        public PermissionService(IApiService apiService, ILogger<PermissionService> logger)
        {
            _apiService = apiService;
            _logger = logger;
            _userPermissionsCache = new ConcurrentDictionary<string, (DateTime, IEnumerable<PermissionModel>)>();
        }

        public async Task<IEnumerable<PermissionModel>> GetAllPermissionsAsync(string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<PermissionModel>>("/api/permissions", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todas as permissões");
                return Enumerable.Empty<PermissionModel>();
            }
        }

        public async Task<PermissionModel> GetPermissionByIdAsync(string id, string token)
        {
            return await _apiService.GetAsync<PermissionModel>($"/api/permissions/{id}", token);
        }

        public async Task<PermissionModel> CreatePermissionAsync(CreatePermissionModel model, string token)
        {
            _userPermissionsCache.Clear();
            return await _apiService.PostAsync<PermissionModel>("/api/permissions", model, token);
        }

        public async Task<PermissionModel> UpdatePermissionAsync(string id, UpdatePermissionModel model, string token)
        {
            _userPermissionsCache.Clear();
            return await _apiService.PutAsync<PermissionModel>($"/api/permissions/{id}", model, token);
        }

        public async Task DeletePermissionAsync(string id, string token)
        {
            _userPermissionsCache.Clear();
            await _apiService.DeleteAsync($"/api/permissions/{id}", token);
        }

        public async Task<IEnumerable<PermissionModel>> GetPermissionsByRoleIdAsync(string roleId, string token)
        {
            try
            {
                return await _apiService.GetAsync<IEnumerable<PermissionModel>>($"/api/permissions/role/{roleId}", token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter permissões para o perfil: {RoleId}", roleId);
                return Enumerable.Empty<PermissionModel>();
            }
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId, string token)
        {
            _userPermissionsCache.Clear();
            return await _apiService.PostAsync<bool>($"/api/permissions/role/{roleId}/permission/{permissionId}", token: token);
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId, string token)
        {
            _userPermissionsCache.Clear();
            return await _apiService.DeleteAsync<bool>($"/api/permissions/role/{roleId}/permission/{permissionId}", token);
        }

        public async Task<IEnumerable<PermissionModel>> GetPermissionsByUserIdAsync(string userId, string token)
        {
            string cacheKey = $"{userId}_{token.GetHashCode()}";

            if (_userPermissionsCache.TryGetValue(cacheKey, out var cachedResult))
            {
                if (DateTime.UtcNow.Subtract(cachedResult.Timestamp).TotalMinutes < 5)
                {
                    _logger.LogDebug("Retornando permissões em cache para o usuário: {UserId}", userId);
                    return cachedResult.Permissions;
                }
            }

            try
            {
                _logger.LogDebug("Buscando permissões da API para o usuário: {UserId}", userId);
                var permissions = await _apiService.GetAsync<IEnumerable<PermissionModel>>($"/api/permissions/user/{userId}", token);

                _userPermissionsCache[cacheKey] = (DateTime.UtcNow, permissions);

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter permissões para o usuário: {UserId}", userId);
                return Enumerable.Empty<PermissionModel>();
            }
        }
    }
}