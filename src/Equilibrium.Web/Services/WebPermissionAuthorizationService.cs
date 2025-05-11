using Equilibrium.Web.Extensions;
using Equilibrium.Web.Interfaces;
using System.Security.Claims;

namespace Equilibrium.Web.Services
{
    public class WebPermissionAuthorizationService : IWebPermissionAuthorizationService
    {
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<WebPermissionAuthorizationService> _logger;

        public WebPermissionAuthorizationService(
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<WebPermissionAuthorizationService> logger)
        {
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionSystemName)
        {
            var context = _httpContextAccessor?.HttpContext!;

            try
            {
                if (!context.IsUserAuthenticated())
                {
                    _logger.LogWarning("Usuário não autenticado tentando acessar recurso que requer permissão: {PermissionName}", permissionSystemName);
                    return false;
                }

                var token = context.GetJwtToken();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token não encontrado para usuário autenticado: {User}", context.GetUserName());
                    return false;
                }

                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogWarning("Claim de ID não encontrada para usuário: {User}", context.GetUserName());
                    return false;
                }

                var permissions = await _permissionService.GetPermissionsByUserIdAsync(userIdClaim.Value, token);
                var hasPermission = permissions.Any(p => p.SystemName == permissionSystemName);

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar permissão {PermissionName} para usuário {User}",
                    permissionSystemName, user.Identity?.Name ?? "unknown");
                return false;
            }
        }
    }
}