namespace DailyHub.Infrastructure.Providers.Calendar;

using DailyHub.Shared.Abstractions.Calendar;
using DailyHub.Shared.Dto.Calendar;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

/// <summary>
/// تعطیلات رسمی کشورها از date.nager.at (رایگان، بدون کلید)
/// برای ایران (که Nager پشتیبانی نمی‌کند) از تقویم شمسی دات‌نت استفاده می‌شود
/// </summary>
public sealed class NagerDateProvider : ICalendarProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<NagerDateProvider> _logger;

    public NagerDateProvider(HttpClient http, ILogger<NagerDateProvider> logger)
    {
        _http = http;
        _logger = logger;
    }

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // تعطیلات رسمی ثابتِ شمسی ایران (ماه/روز جلالی)
    private static readonly (int M, int D, string Local, string En)[] IranianSolarHolidays =
    [
        (1, 1, "نوروز (آغاز سال نو)", "Nowruz"),
        (1, 2, "عید نوروز", "Nowruz Holiday"),
        (1, 3, "عید نوروز", "Nowruz Holiday"),
        (1, 4, "عید نوروز", "Nowruz Holiday"),
        (1, 12, "روز جمهوری اسلامی", "Islamic Republic Day"),
        (1, 13, "روز طبیعت", "Nature Day"),
        (3, 14, "رحلت امام خمینی", "Demise of Imam Khomeini"),
        (3, 15, "قیام ۱۵ خرداد", "Khordad 15 Uprising"),
        (11, 22, "پیروزی انقلاب اسلامی", "Islamic Revolution Victory Day"),
        (12, 29, "ملی شدن صنعت نفت", "Oil Nationalization Day")
    ];

    public async Task<IReadOnlyList<HolidayDto>> GetHolidaysAsync(string countryCode, int year, CancellationToken ct = default)
    {
        var cc = string.IsNullOrWhiteSpace(countryCode) ? "IR" : countryCode.Trim().ToUpperInvariant();
        if (year < 1900 || year > 2100) year = DateTime.UtcNow.Year;

        // ایران: تولید از تقویم شمسی (مناسبت‌های قمری در این لیست نیست)
        if (cc == "IR")
            return IranianHolidaysForYear(year);

        var url = $"https://date.nager.at/api/v3/PublicHolidays/{year}/{Uri.EscapeDataString(cc)}";

        using var res = await _http.GetAsync(url, ct);

        if (res.StatusCode == System.Net.HttpStatusCode.NoContent || res.StatusCode == System.Net.HttpStatusCode.NotFound)
            return Array.Empty<HolidayDto>();

        res.EnsureSuccessStatusCode();

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        var raw = await JsonSerializer.DeserializeAsync<List<NagerHoliday>>(stream, _json, ct) ?? new List<NagerHoliday>();

        return raw
            .Select(h => new HolidayDto
            {
                Date = DateTime.TryParse(h.Date, out var d) ? d : default,
                LocalName = h.LocalName ?? "",
                Name = h.Name ?? "",
                CountryCode = h.CountryCode ?? cc
            })
            .OrderBy(h => h.Date)
            .ToList();
    }

    /// <summary>هر سال میلادی با دو سال شمسی هم‌پوشانی دارد؛ هر دو را پوشش می‌دهیم</summary>
    private static List<HolidayDto> IranianHolidaysForYear(int gregorianYear)
    {
        var pc = new PersianCalendar();
        var result = new List<HolidayDto>();

        foreach (var jy in new[] { gregorianYear - 621, gregorianYear - 622 })
        {
            foreach (var (jm, jd, local, en) in IranianSolarHolidays)
            {
                var g = pc.ToDateTime(jy, jm, jd, 12, 0, 0, 0);
                if (g.Year != gregorianYear) continue;
                result.Add(new HolidayDto
                {
                    Date = g.Date,
                    LocalName = local,
                    Name = en,
                    CountryCode = "IR"
                });
            }
        }

        return result.OrderBy(h => h.Date).ToList();
    }

    private sealed class NagerHoliday
    {
        public string? Date { get; set; }
        public string? LocalName { get; set; }
        public string? Name { get; set; }
        public string? CountryCode { get; set; }
        public bool Global { get; set; }
        public List<string>? Counties { get; set; }
    }
}
