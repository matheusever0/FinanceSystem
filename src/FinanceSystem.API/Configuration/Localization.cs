using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace FinanceSystem.API.Configuration
{
    public static class Localization
    {
        public static IServiceCollection AddLocalizationResources(this IServiceCollection services)
        {
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] {
                new CultureInfo("en-US"),
                new CultureInfo("pt-BR")
            };

                options.DefaultRequestCulture = new RequestCulture("pt-BR");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            };

            });

            return services;
        }
    }
}
