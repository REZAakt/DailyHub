namespace DailyHub.Infrastructure.Providers.Rates;

using DailyHub.Shared.Abstractions.Rates;
using DailyHub.Shared.Dto.Rates;
using System.Net.Http.Json;
using System.Text.Json;

public sealed class FrankfurterRatesProvider : IRatesProvider
{
    private readonly HttpClient _http;

    public FrankfurterRatesProvider(HttpClient http)
    {
        _http = http;
    }

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<FxQuoteDto>> GetRatesAsync(string baseCurrency, IEnumerable<string> symbols, CancellationToken ct = default)
    {
        var @base = string.IsNullOrWhiteSpace(baseCurrency) ? "USD" : baseCurrency.Trim().ToUpperInvariant();
        var list = (symbols ?? Enumerable.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToUpperInvariant())
            .Where(s => s != @base)
            .Distinct()
            .ToList();

        var url = $"https://api.frankfurter.app/latest?from={Uri.EscapeDataString(@base)}";
        if (list.Count > 0)
            url += "&to=" + Uri.EscapeDataString(string.Join(",", list));

        using var res = await _http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        var raw = await JsonSerializer.DeserializeAsync<FrankfurterResponse>(stream, _json, ct);

        if (raw is null || raw.Rates is null || raw.Rates.Count == 0)
            return Array.Empty<FxQuoteDto>();

        var ts = (raw.Date ?? DateTime.UtcNow).ToUniversalTime();

        return raw.Rates
            .Select(kv => new FxQuoteDto
            {
                Base = @base,
                Symbol = kv.Key,
                Rate = kv.Value,
                TimestampUtc = ts
            })
            .OrderBy(q => q.Symbol)
            .ToList();
    }

    private sealed class FrankfurterResponse
    {
        public decimal Amount { get; set; }
        public string? Base { get; set; }
        public DateTime? Date { get; set; }
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
