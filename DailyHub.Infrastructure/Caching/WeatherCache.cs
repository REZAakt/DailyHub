using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Caching;

using DailyHub.Shared.Abstractions.Weather;
using DailyHub.Shared.Dto.Weather;
using Microsoft.Extensions.Caching.Memory;

internal sealed class WeatherCache(IMemoryCache cache, IWeatherProvider provider) : IWeatherCache
{
    public async Task<WeatherForecastDto> GetOrFetchAsync(
        double lat, double lon, int days, TimeSpan maxAge, CancellationToken ct = default)
    {
        var key = $"wx:{lat:F4}:{lon:F4}:{days}";
        if (cache.TryGetValue(key, out WeatherForecastDto dto))
            return dto;

        dto = await provider.GetForecastAsync(lat, lon, days, ct);
        cache.Set(key, dto, maxAge); // absolute expiration
        return dto;
    }
}
