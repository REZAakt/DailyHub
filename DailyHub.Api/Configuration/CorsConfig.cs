namespace DailyHub.Api.Configuration;

using Microsoft.Extensions.DependencyInjection;

public static class CorsConfig
{
    public static IServiceCollection AddCorsConfig(this IServiceCollection services)
    {
        services.AddCors(o => o.AddDefaultPolicy(p => p
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()));
        return services;
    }
}
