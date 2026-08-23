namespace DailyHub.Infrastructure.Providers.Weather;

using System.Net.Http.Json;
using System.Text.Json;
using DailyHub.Infrastructure.ExternalModels.OpenMeteo;
using DailyHub.Infrastructure.Http;
using DailyHub.Shared.Abstractions.Weather;
using DailyHub.Shared.Dto.Weather;

public sealed class OpenMeteoProvider(HttpClient http) : IWeatherProvider
{
    public async Task<WeatherForecastDto> GetForecastAsync(double latitude, double longitude, int days = 7, CancellationToken ct = default)
    {
        // daily: min/max/precip/weathercode + current_weather
        var url = $"v1/forecast?latitude={latitude}&longitude={longitude}" +
                  $"&current_weather=true" +
                  $"&timezone=auto" +
                  $"&forecast_days={Math.Clamp(days, 1, 16)}" +
                  $"&daily=temperature_2m_max,temperature_2m_min,precipitation_sum,weathercode";

        var res = await http.GetFromJsonAsync<OpenMeteoResponse>(url, JsonOptions(), ct)
                  ?? throw new InvalidOperationException("Empty response from Open-Meteo.");

        var dto = new WeatherForecastDto
        {
            Latitude = res.latitude,
            Longitude = res.longitude,
            Now = new WeatherNowDto
            {
                AsOfUtc = res.current_weather.time.ToUniversalTime(),
                TemperatureC = res.current_weather.temperature,
                WindKph = res.current_weather.windspeed,
                WeatherCode = res.current_weather.weathercode,
            },
            Daily = MergeDaily(res)
        };

        // today (با فرمت yyyy-MM-dd چون Date را string کرده‌ایم)
        var todayIso = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
        var today = dto.Daily.FirstOrDefault(d => d.Date == todayIso);
        if (today is not null)
        {
            dto.Now.TodayMinC = today.MinC;
            dto.Now.TodayMaxC = today.MaxC;
            dto.Now.TodayPrecipMm = today.PrecipMm;
        }

        return dto;
    }

    private static IReadOnlyList<WeatherDayDto> MergeDaily(OpenMeteoResponse r)
    {
        int count = r.daily.time.Count;
        count = Math.Min(count, r.daily.temperature_2m_min.Count);
        count = Math.Min(count, r.daily.temperature_2m_max.Count);
        count = Math.Min(count, r.daily.precipitation_sum.Count);
        count = Math.Min(count, r.daily.weathercode.Count);

        var list = new List<WeatherDayDto>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new WeatherDayDto
            {
                Date = r.daily.time[i], // "YYYY-MM-DD"
                MinC = r.daily.temperature_2m_min[i],
                MaxC = r.daily.temperature_2m_max[i],
                PrecipMm = r.daily.precipitation_sum[i],
                WeatherCode = r.daily.weathercode[i],
            });
        }
        return list;
    }

    private static double SafeAt(List<double> arr, int i) => (i < arr.Count ? arr[i] : double.NaN);
    private static int SafeAt(List<int> arr, int i) => (i < arr.Count ? arr[i] : 0);

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true
    };
}
