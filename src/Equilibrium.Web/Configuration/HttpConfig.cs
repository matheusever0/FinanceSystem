namespace Equilibrium.Web.Configuration
{
    public static class HttpConfig
    {
        public static IServiceCollection AddHttpConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("FinanceAPI", client =>
            {
                client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]!);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });

            return services;
        }
    }
}
