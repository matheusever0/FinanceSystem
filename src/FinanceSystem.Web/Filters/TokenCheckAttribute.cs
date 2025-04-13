using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

namespace FinanceSystem.Web.Filters
{
    public class TokenCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            if (httpContext.User.Identity.IsAuthenticated)
            {
                var token = httpContext.Session.GetString("JWToken");

                if (string.IsNullOrEmpty(token))
                {
                    // Token ausente mas usuário autenticado - limpar autenticação
                    httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
                    httpContext.Session.Clear();

                    context.Result = new RedirectToActionResult("Login", "Account", new { expired = true });
                    return;
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
