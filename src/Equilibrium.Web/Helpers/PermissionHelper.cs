using Equilibrium.Web.Extensions;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Permission;
using System.Security.Claims;

namespace Equilibrium.Web.Helpers
{
    public static class PermissionHelper
    {
        public static async Task<bool> HasPermissionAsync(HttpContext httpContext, string permissionSystemName)
        {
            var logger = httpContext.RequestServices.GetService<ILogger<object>>();

            try
            {
                if (!httpContext.IsUserAuthenticated())
                {
                    logger?.LogWarning("Usuário não autenticado tentando acessar {Permission}", permissionSystemName);
                    return false;
                }

                var permissionService = httpContext.RequestServices.GetRequiredService<IPermissionService>();
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                var token = httpContext.GetJwtToken();

                if (userIdClaim == null || string.IsNullOrEmpty(token))
                {
                    logger?.LogWarning("Dados de usuário incompletos para verificação de permissão");
                    return false;
                }

                var permissions = await permissionService.GetPermissionsByUserIdAsync(userIdClaim.Value, token);

                var hasPermission = permissions.Any(p => p.SystemName == permissionSystemName);

                return hasPermission;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Erro ao verificar permissão {Permission}", permissionSystemName);
                return false;
            }
        }

        public static async Task<bool> UserHasAnyPermissionAsync(
            HttpContext httpContext,
            params string[] permissionSystemNames)
        {
            foreach (var permission in permissionSystemNames)
            {
                if (await HasPermissionAsync(httpContext, permission))
                    return true;
            }
            return false;
        }


        public static bool PodeEditarSomenteProprioUsuario(this IEnumerable<PermissionModel> permissions)
        {
            return permissions.Any(e => e.SystemName == "users.edit.unique") &&
                   !permissions.Any(e => e.SystemName != "users.edit.unique" && e.SystemName.Contains("users.edit"));
        }


    }
}