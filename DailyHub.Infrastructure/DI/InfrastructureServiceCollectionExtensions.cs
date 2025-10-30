using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.DI;

using DailyHub.Infrastructure.Caching;
using DailyHub.Infrastructure.Http;
using DailyHub.Infrastructure.Providers.Weather;
using DailyHub.Shared.Abstractions.Weather;
using Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddDailyHubInfrastructure(this IServiceCollection services)
    {
        //services.AddHttpClient(HttpClientNames.OpenMeteo, c =>
        //{
        //    c.BaseAddress = new Uri("https://api.open-meteo.com/");
        //    c.DefaultRequestHeaders.UserAgent.ParseAdd("DailyHub/1.0 (+github.com/yourrepo)");
        //});

        //services.AddScoped<IWeatherProvider, OpenMeteoProvider>(sp =>
        //{
        //    var factory = sp.GetRequiredService<IHttpClientFactory>();
        //    return new OpenMeteoProvider(factory.CreateClient(HttpClientNames.OpenMeteo));
        //});

        services.AddHttpClient(HttpClientNames.OpenMeteo, c =>
        {
            c.BaseAddress = new Uri("https://api.open-meteo.com/");
            c.DefaultRequestHeaders.UserAgent.ParseAdd("DailyHub/1.0");
        });
        services.AddScoped<IWeatherProvider, OpenMeteoProvider>(sp =>
        {
            var f = sp.GetRequiredService<IHttpClientFactory>();
            return new OpenMeteoProvider(f.CreateClient(HttpClientNames.OpenMeteo));
        });

        services.AddScoped<IWeatherCache, WeatherCache>();

        return services;
    }
}
