using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;

namespace FinanceSystem.Web.Configuration
{
    public static class WebConfig
    {
        public static IServiceCollection AddWebConfiguration(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(24);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.MaxAge = TimeSpan.FromHours(24);
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