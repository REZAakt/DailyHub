using DailyHub.Shared.Abstractions.Weather;
using DailyHub.Shared.Dto.Weather;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DailyHub.Infrastructure.Caching;

namespace DailyHub.Api.Hubs;

public sealed class WeatherHub : Hub
{
    private readonly IWeatherCache _cache;
    private readonly ILogger<WeatherHub> _logger;

    public WeatherHub(IWeatherCache cache, ILogger<WeatherHub> logger)
    {
        _cache = cache; _logger = logger;
    }

    public async IAsyncEnumerable<WeatherForecastDto> Subscribe(
        double lat, double lon, int days = 7,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        WeatherForecastDto dto;
        try
        {
            dto = await _cache.GetOrFetchAsync(lat, lon, days, TimeSpan.FromMinutes(30), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Subscribe failed {Lat},{Lon},{Days}", lat, lon, days);
            throw;
        }

        yield return dto; // یک‌بار ارسال
    }
}
