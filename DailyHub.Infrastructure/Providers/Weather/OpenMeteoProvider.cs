using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Providers.Weather;

using DailyHub.Shared.Abstractions.Weather;
using DailyHub.Shared.Dto.Weather;

public sealed class OpenMeteoProvider : IWeatherProvider
{
    public Task<WeatherSummaryDto> GetSummaryAsync(double latitude, double longitude, CancellationToken ct = default)
        => throw new NotImplementedException();
}
