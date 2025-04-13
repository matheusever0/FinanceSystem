using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceSystem.Web.Helpers
{
    public static class PermissionHelper
    {
        public static async Task<bool> HasPermissionAsync(this Controller controller, string permissionSystemName)
        {
            return await HasPermissionAsync(controller.HttpContext, permissionSystemName);
        }

        public static async Task<bool> HasPermissionAsync(HttpContext httpContext, string permissionSystemName)
        {
            // Obter logger para diagnóstico
            var logger = httpContext.RequestServices.GetService<ILogger<object>>();
            logger?.LogInformation("Verificando permissão: {Permission}", permissionSystemName);

            // Verificar se o usuário está autenticado
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                logger?.LogWarning("Usuário não autenticado tentando verificar permissão {Permission}", permissionSystemName);
                return false;
            }

            // Admin tem todas as permissões
            if (httpContext.User.IsInRole("Admin"))
            {
                logger?.LogInformation("Usuário admin - permissão concedida para {Permission}", permissionSystemName);
                return true;
            }

            var token = httpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                logger?.LogWarning("Token não encontrado ao verificar permissão {Permission}", permissionSystemName);
                return false;
            }

            try
            {
                // Obter o serviço de permissão
                var permissionService = httpContext.RequestServices.GetService<IPermissionService>();
                if (permissionService == null)
                {
                    logger?.LogError("Serviço de permissões não disponível");
                    return false;
                }

                // Obter o ID do usuário
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    logger?.LogWarning("Claim de ID não encontrada para usuário");
                    return false;
                }

                // Mapeamento simplificado de permissões baseado no perfil
                // Este é um fallback caso a API retorne erro
                if (httpContext.User.IsInRole("Moderator"))
                {
                    // Perfil básico para moderadores
                    var moderatorPermissions = new[] {
                        "users.view", "users.create", "users.edit",
                        "roles.view"
                    };

                    if (moderatorPermissions.Contains(permissionSystemName))
                    {
                        logger?.LogInformation("Permissão concedida via fallback para moderador: {Permission}", permissionSystemName);
                        return true;
                    }
                }

                // Tentar recuperar as permissões da API
                try
                {
                    // Obter as permissões do usuário
                    var permissions = await permissionService.GetPermissionsByUserIdAsync(userIdClaim.Value, token);
                    var hasPermission = permissions.Any(p => p.SystemName == permissionSystemName);

                    logger?.LogInformation("Verificação de permissão via API para {Permission}: {Result}",
                        permissionSystemName, hasPermission ? "concedida" : "negada");

                    return hasPermission;
                }
                catch (Exception apiEx)
                {
                    logger?.LogError(apiEx, "Erro ao consultar permissões na API. Usando fallback.");

                    // Fallback básico para usuário normal
                    if (httpContext.User.IsInRole("User"))
                    {
                        var userPermissions = new[] { "users.view" };
                        return userPermissions.Contains(permissionSystemName);
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Erro ao verificar permissão {Permission}", permissionSystemName);
                return false;
            }
        }

        public static async Task<bool> UserHasAnyPermissionAsync(HttpContext httpContext, string[] permissionSystemNames)
        {
            // Verificar se o usuário está autenticado
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            // Admin tem todas as permissões
            if (httpContext.User.IsInRole("Admin"))
                return true;

            // Verificar cada permissão
            foreach (var permission in permissionSystemNames)
            {
                if (await HasPermissionAsync(httpContext, permission))
                    return true;
            }

            return false;
        }
    }
}