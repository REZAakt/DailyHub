using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Abstractions.Weather;

using DailyHub.Shared.Dto.Weather;

public interface IWeatherProvider
{
    Task<WeatherSummaryDto> GetSummaryAsync(double latitude, double longitude, CancellationToken ct = default);
}
