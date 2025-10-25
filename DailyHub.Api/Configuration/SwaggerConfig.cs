namespace DailyHub.Api.Configuration;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer(); // از Microsoft.AspNetCore.OpenApi می‌آید
        services.AddSwaggerGen();           // از Swashbuckle.AspNetCore می‌آید
        return services;
    }

    public static IApplicationBuilder UseSwaggerConfig(this IApplicationBuilder app)
    {
        app.UseSwagger();                   // از Swashbuckle.AspNetCore
        app.UseSwaggerUI();
        return app;
    }
}
