namespace DailyHub.Api.Endpoints;

using DailyHub.Shared.Abstractions.Weather;

public static class WeatherEndpoints
{
    public static IEndpointRouteBuilder MapWeatherEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/weather/forecast", async (double lat, double lon, int days, IWeatherProvider p, CancellationToken ct) =>
        {
            try
            {
                var dto = await p.GetForecastAsync(lat, lon, days <= 0 ? 7 : days, ct);
                return Results.Ok(dto);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.ToString());
            }
        });

        // برای تست سریع مختصات بابل
        app.MapGet("/api/weather/diag-babol", async (IWeatherProvider p, CancellationToken ct) =>
        {
            var dto = await p.GetForecastAsync(36.5513, 52.6793, 7, ct);
            return Results.Ok(dto);
        });

        return app;
    }
}
