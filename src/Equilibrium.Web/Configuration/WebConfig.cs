using Equilibrium.Web.Extensions;
using Equilibrium.Web.Filters;

namespace Equilibrium.Web.Configuration
{
    public static class WebConfig
    {
        public static IServiceCollection AddWebConfiguration(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddDistributedMemoryCache();

            // Configuração de sessão alinhada com os cookies de autenticação (24 horas)
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1); // 24 horas
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.MaxAge = TimeSpan.FromDays(1); // 24 horas
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = "EquilibriumSession";
            });

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<TokenCheckAttribute>();
            });

            return services;
        }

        public static IApplicationBuilder UseWebConfiguration(this IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthenticationMiddleware();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            return app;
        }
    }
}