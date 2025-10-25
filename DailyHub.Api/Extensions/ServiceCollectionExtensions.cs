namespace DailyHub.Api.Extensions;

using DailyHub.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
        => services.AddSwaggerConfig()
                   .AddCorsConfig()
                   .AddJsonConfig();
}
