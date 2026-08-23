using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.Weather;

//public sealed class WeatherSummaryDto
//{
//    public double TemperatureC { get; set; }
//    public double WindKph { get; set; }
//    public DateTime TimestampUtc { get; set; }
//}


public sealed class WeatherForecastDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public WeatherNowDto Now { get; set; } = new();
    public IReadOnlyList<WeatherDayDto> Daily { get; set; } = Array.Empty<WeatherDayDto>();
}


public sealed class WeatherNowDto
{
    public DateTime AsOfUtc { get; set; }
    public double TemperatureC { get; set; }
    public double WindKph { get; set; }
    public int WeatherCode { get; set; }

    // خلاصه‌ی امروز (برای کارت بالای صفحه)
    public double TodayMinC { get; set; }
    public double TodayMaxC { get; set; }
    public double TodayPrecipMm { get; set; }
}


public sealed class WeatherDayDto
{
    public string Date { get; set; } = "";
    public double MinC { get; set; }
    public double MaxC { get; set; }
    public double PrecipMm { get; set; }
    public int WeatherCode { get; set; }
}
