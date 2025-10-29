namespace DailyHub.Api.Hubs;

using DailyHub.Shared.Abstractions.Weather;
using DailyHub.Shared.Dto.Weather;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public sealed class WeatherHub : Hub
{
    private readonly IWeatherProvider _provider;
    private readonly ILogger<WeatherHub> _logger;

    public WeatherHub(IWeatherProvider provider, ILogger<WeatherHub> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async IAsyncEnumerable<WeatherForecastDto> Subscribe(
        double lat, double lon, int days = 7,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        WeatherForecastDto? dto = null;

        try
        {
            dto = await _provider.GetForecastAsync(lat, lon, days, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Subscribe failed for {Lat},{Lon},{Days}", lat, lon, days);
            throw; // اجازه بده exception به کلاینت برگردد
        }

        if (dto is not null)
            yield return dto;
    }

}
