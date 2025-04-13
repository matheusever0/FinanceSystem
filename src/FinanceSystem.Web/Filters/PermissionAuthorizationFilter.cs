using FinanceSystem.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinanceSystem.Web.Filters
{
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IWebPermissionAuthorizationService _permissionAuthorizationService;
        private readonly string _permissionSystemName;
        private readonly ILogger<PermissionAuthorizationFilter> _logger;

        public PermissionAuthorizationFilter(
            IWebPermissionAuthorizationService permissionAuthorizationService,
            ILogger<PermissionAuthorizationFilter> logger,
            string permissionSystemName)
        {
            _permissionAuthorizationService = permissionAuthorizationService;
            _permissionSystemName = permissionSystemName;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            _logger.LogInformation("Verificando permissão: {Permission}", _permissionSystemName);

            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Acesso negado: usuário não autenticado tentando acessar recurso com permissão {Permission}", _permissionSystemName);
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // Bypass para administradores
            if (context.HttpContext.User.IsInRole("Admin"))
            {
                _logger.LogInformation("Acesso concedido: usuário admin {User} acessando recurso",
                    context.HttpContext.User.Identity.Name);
                return;
            }

            if (!await _permissionAuthorizationService.HasPermissionAsync(context.HttpContext.User, _permissionSystemName))
            {
                _logger.LogWarning("Acesso negado: usuário {User} não possui permissão {Permission}",
                    context.HttpContext.User.Identity.Name, _permissionSystemName);

                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            _logger.LogInformation("Acesso concedido: usuário {User} possui permissão {Permission}",
                context.HttpContext.User.Identity.Name, _permissionSystemName);
        }
    }
}