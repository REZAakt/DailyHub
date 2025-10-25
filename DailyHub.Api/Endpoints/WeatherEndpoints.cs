namespace DailyHub.Api.Endpoints;

using DailyHub.Shared.Abstractions.Weather;
using DailyHub.Shared.Dto.Weather;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

public static class WeatherEndpoints
{
    public static IEndpointRouteBuilder MapWeatherEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/weather", (double lat, double lon, IWeatherProvider provider) =>
        {
            // Implement later
            throw new NotImplementedException();
        });

        return app;
    }
}
