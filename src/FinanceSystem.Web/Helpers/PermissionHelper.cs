using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.Web.Helpers
{
    public static class PermissionHelper
    {
        public static async Task<bool> HasPermissionAsync(this Controller controller, string permissionSystemName)
        {
            // Verificar se o usuário está autenticado
            if (!controller.User.Identity.IsAuthenticated)
                return false;

            // Admin tem todas as permissões
            if (controller.User.IsInRole("Admin"))
                return true;

            var httpContext = controller.HttpContext;
            var token = httpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                // Obter o serviço de permissão
                var permissionService = httpContext.RequestServices.GetService<IPermissionService>();
                if (permissionService == null)
                    return false;

                // Obter o ID do usuário
                var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return false;

                // Obter as permissões do usuário
                var permissions = await permissionService.GetPermissionsByUserIdAsync(userIdClaim.Value, token);

                // Verificar se o usuário tem a permissão necessária
                return permissions.Any(p => p.SystemName == permissionSystemName);
            }
            catch
            {
                return false;
            }
        }
    }
}