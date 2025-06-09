using Microsoft.AspNetCore.Authentication.Cookies;

namespace Equilibrium.Web.Configuration
{
    public static class AuthConfig
    {
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.LogoutPath = "/Account/Logout";

                    options.ExpireTimeSpan = TimeSpan.FromDays(1);

                    options.SlidingExpiration = true;

                    options.Cookie.Name = "EquilibriumAuth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;

                    options.Cookie.MaxAge = TimeSpan.FromDays(1);

                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetService<ILogger<object>>();
                            logger?.LogDebug("Validando principal. Expira em: {ExpiresUtc}",
                                context.Properties.ExpiresUtc);
                            return Task.CompletedTask;
                        },
                        OnRedirectToLogin = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetService<ILogger<object>>();
                            logger?.LogInformation("Redirecionando para login. Path: {Path}",
                                context.Request.Path);

                            if (context.Request.Headers.XRequestedWith == "XMLHttpRequest" ||
                                context.Request.Headers.Accept.ToString().Contains("application/json"))
                            {
                                context.Response.StatusCode = 401;
                                return Task.CompletedTask;
                            }

                            context.Response.Redirect(context.RedirectUri);
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetService<ILogger<object>>();
                            logger?.LogWarning("Acesso negado para path: {Path}", context.Request.Path);

                            context.Response.Redirect(context.RedirectUri);
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}