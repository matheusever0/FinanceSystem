﻿using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Equilibrium.API.Configuration
{
    public static class Localization
    {
        public static IServiceCollection AddLocalizationResources(this IServiceCollection services)
        {
            services.AddLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] {
                new CultureInfo("en-US"),
                new CultureInfo("pt-BR")
            };

                options.DefaultRequestCulture = new RequestCulture("pt-BR");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                options.RequestCultureProviders =
            [
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            ];

            });

            return services;
        }
    }
}
