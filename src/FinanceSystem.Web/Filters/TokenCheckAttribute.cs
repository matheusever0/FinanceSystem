using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using FinanceSystem.Web.Extensions;
using System.IdentityModel.Tokens.Jwt;

namespace FinanceSystem.Web.Filters
{
    public class TokenCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            if (httpContext.IsUserAuthenticated())
            {
                var token = httpContext.GetJwtToken();

                if (string.IsNullOrEmpty(token) || !IsTokenValid(token))
                {
                    httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
                    httpContext.Session.Clear();

                    context.Result = new RedirectToActionResult("Login", "Account", new { expired = true });
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

        private static bool IsTokenValid(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                if (!tokenHandler.CanReadToken(token))
                    return false;

                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }
    }
}
