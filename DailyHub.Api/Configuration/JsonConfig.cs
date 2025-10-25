namespace DailyHub.Api.Configuration;

using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

public static class JsonConfig
{
    public static IServiceCollection AddJsonConfig(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            o.SerializerOptions.WriteIndented = false;
        });
        return services;
    }
}
