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

                    // Configurações de expiração - 24 horas
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);

                    // Renovação automática quando mais da metade do tempo passou
                    options.SlidingExpiration = true;

                    // Configurações do cookie
                    options.Cookie.Name = "EquilibriumAuth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;

                    // Definir tempo de vida do cookie igual ao da sessão
                    options.Cookie.MaxAge = TimeSpan.FromDays(1);

                    // Configurações de eventos para debug
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnValidatePrincipal = async context =>
                        {
                            // Log para debug
                            var logger = context.HttpContext.RequestServices.GetService<ILogger<object>>();
                            logger?.LogDebug("Validando principal. Expira em: {ExpiresUtc}",
                                context.Properties.ExpiresUtc);
                        },
                        OnRedirectToLogin = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetService<ILogger<object>>();
                            logger?.LogInformation("Redirecionando para login. Path: {Path}",
                                context.Request.Path);

                            // Para requisições AJAX, retornar 401 ao invés de redirecionar
                            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                                context.Request.Headers["Accept"].ToString().Contains("application/json"))
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