using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.Weather;

public sealed class WeatherSummaryDto
{
    public double TemperatureC { get; set; }
    public double WindKph { get; set; }
    public DateTime TimestampUtc { get; set; }
}
