using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Caching;
using DailyHub.Shared.Dto.Weather;

public interface IWeatherCache
{
    Task<WeatherForecastDto> GetOrFetchAsync(
        double lat, double lon, int days, TimeSpan maxAge, CancellationToken ct = default);
}
