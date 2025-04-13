using FinanceSystem.Web.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IWebPermissionAuthorizationService _permissionAuthorizationService;
    private readonly string _permissionSystemName;

    public PermissionAuthorizationFilter(
        IWebPermissionAuthorizationService permissionAuthorizationService,
        string permissionSystemName)
    {
        _permissionAuthorizationService = permissionAuthorizationService;
        _permissionSystemName = permissionSystemName;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.Identity.IsAuthenticated)
        {
            context.Result = new ChallengeResult();
            return;
        }

        if (!await _permissionAuthorizationService.HasPermissionAsync(context.HttpContext.User, _permissionSystemName))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}