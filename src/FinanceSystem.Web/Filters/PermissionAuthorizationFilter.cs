using FinanceSystem.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinanceSystem.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permissionSystemName) : base(typeof(PermissionAuthorizationFilter))
        {
            Arguments = new object[] { permissionSystemName };
        }
    }

    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionAuthorizationService _permissionAuthorizationService;
        private readonly string _permissionSystemName;

        public PermissionAuthorizationFilter(
            IPermissionAuthorizationService permissionAuthorizationService,
            string permissionSystemName)
        {
            _permissionAuthorizationService = permissionAuthorizationService;
            _permissionSystemName = permissionSystemName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Verificar se o usuário está autenticado
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // Verificar se o usuário tem a permissão necessária
            if (!await _permissionAuthorizationService.HasPermissionAsync(context.HttpContext.User, _permissionSystemName))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}