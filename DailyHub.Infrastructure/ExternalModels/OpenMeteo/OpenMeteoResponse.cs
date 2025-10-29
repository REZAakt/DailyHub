using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.ExternalModels.OpenMeteo;
public sealed class OpenMeteoResponse
{
    public double latitude { get; set; }
    public double longitude { get; set; }
    public CurrentWeather current_weather { get; set; } = new();
    public DailyUnits daily_units { get; set; } = new();
    public Daily daily { get; set; } = new();

    public sealed class CurrentWeather
    {
        public double temperature { get; set; }
        public double windspeed { get; set; }
        public int weathercode { get; set; }
        public DateTime time { get; set; }
    }

    public sealed class DailyUnits
    {
        public string time { get; set; } = "iso8601";
        public string temperature_2m_max { get; set; } = "°C";
        public string temperature_2m_min { get; set; } = "°C";
        public string precipitation_sum { get; set; } = "mm";
        public string weathercode { get; set; } = "wmo";
    }

    public sealed class Daily
    {
        // ⬅ به string تغییر دادیم تا مطمئن باشیم
        public List<string> time { get; set; } = new();
        public List<double> temperature_2m_max { get; set; } = new();
        public List<double> temperature_2m_min { get; set; } = new();
        public List<double> precipitation_sum { get; set; } = new();
        public List<int> weathercode { get; set; } = new();
    }
}
