using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Abstractions.Weather;

using DailyHub.Shared.Dto.Weather;

public interface IWeatherProvider
{
    /// <summary>Forecast شامل وضعیت الان، امروز و 7 روز آینده</summary>
    Task<WeatherForecastDto> GetForecastAsync(double latitude, double longitude, int days = 7, CancellationToken ct = default);
}
