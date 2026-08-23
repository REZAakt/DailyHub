namespace DailyHub.Infrastructure.Providers.Metals;

using DailyHub.Shared.Abstractions.Metals;
using DailyHub.Shared.Dto.Metals;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// قیمت لحظه‌ای فلزات گران‌بها از gold-api.com (رایگان، بدون کلید)
/// </summary>
public sealed class MetalsLiveProvider : IMetalsProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<MetalsLiveProvider> _logger;

    public MetalsLiveProvider(HttpClient http, ILogger<MetalsLiveProvider> logger)
    {
        _http = http;
        _logger = logger;
    }

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // نمادهای پشتیبانی‌شده توسط سرویس + نام فارسی
    public static readonly IReadOnlyDictionary<string, string> KnownSymbols = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["XAU"] = "طلا (اونس)",
        ["XAG"] = "نقره (اونس)",
        ["XPT"] = "پلاتین (اونس)",
        ["XPD"] = "پالادیم (اونس)"
    };

    public async Task<IReadOnlyList<MetalQuoteDto>> GetSpotsAsync(IEnumerable<string> symbols, CancellationToken ct = default)
    {
        var wanted = (symbols ?? Enumerable.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (wanted.Count == 0)
            wanted = KnownSymbols.Keys.ToList();

        var tasks = wanted.Select(async sym =>
        {
            try
            {
                using var res = await _http.GetAsync($"https://api.gold-api.com/price/{sym}", ct);
                if (!res.IsSuccessStatusCode) return null;

                await using var stream = await res.Content.ReadAsStreamAsync(ct);
                var raw = await JsonSerializer.DeserializeAsync<GoldApiResponse>(stream, _json, ct);
                if (raw is null || raw.Price <= 0) return null;

                return new MetalQuoteDto
                {
                    Symbol = sym,
                    Price = Convert.ToDecimal(raw.Price),
                    QuoteCurrency = "USD",
                    TimestampUtc = DateTime.TryParse(raw.UpdatedAt, out var t) ? t.ToUniversalTime() : DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Metals fetch failed for {Symbol}", sym);
                return null;
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(r => r is not null).Cast<MetalQuoteDto>().ToList();
    }

    private sealed class GoldApiResponse
    {
        public string? Name { get; set; }
        public double Price { get; set; }
        public string? Symbol { get; set; }
        public string? UpdatedAt { get; set; }
        public string? UpdatedAtReadable { get; set; }
    }
}
