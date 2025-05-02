using Equilibrium.API.Middlewares;
using Equilibrium.Application;
using Equilibrium.Infrastructure;

namespace Equilibrium.API.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
            });

            services.AddApplication();
            services.AddInfrastructure(configuration);
            services.AddEndpointsApiExplorer();

            return services;
        }

        public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Finance System API v1");
            });

            app.UseHttpsRedirection();
            app.UseCors("AllowSpecificOrigin");
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseRouting();
            app.UseHttpMethodOverride();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            return app;
        }
    }
}