using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinanceSystem.Web.Filters
{
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IWebPermissionAuthorizationService _permissionService;
        private readonly ILogger<PermissionAuthorizationFilter> _logger;
        private readonly string _permissionSystemName;

        public PermissionAuthorizationFilter(
            IWebPermissionAuthorizationService permissionService,
            ILogger<PermissionAuthorizationFilter> logger,
            string permissionSystemName)
        {
            _permissionService = permissionService;
            _logger = logger;
            _permissionSystemName = permissionSystemName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            _logger.LogInformation("Verificando permissão: {Permission}", _permissionSystemName);

            if (!context.HttpContext.IsUserAuthenticated())
            {
                _logger.LogWarning("Acesso negado: usuário não autenticado tentando acessar recurso com permissão {Permission}", _permissionSystemName);
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            var permissions = _permissionSystemName.Split(',').Select(p => p.Trim());
            var hasAnyPermission = false;

            foreach (var permission in permissions)
            {
                if (await _permissionService.HasPermissionAsync(context.HttpContext.User, permission))
                {
                    hasAnyPermission = true;
                    break;
                }
            }

            if (!hasAnyPermission)
            {
                _logger.LogWarning("Acesso negado: usuário {User} não possui nenhuma das permissões: {Permission}",
                context.HttpContext.GetUserName(), _permissionSystemName);
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            _logger.LogInformation("Acesso concedido: usuário {User} possui ao menos uma das permissões: {Permission}",
                context.HttpContext.GetUserName(), _permissionSystemName);
        }
    }
}