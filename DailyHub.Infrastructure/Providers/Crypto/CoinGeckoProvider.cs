using DailyHub.Shared.Abstractions.Crypto;
using DailyHub.Shared.Dto.Crypto;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;


public sealed class CoinGeckoProvider : ICryptoProvider
{
    private readonly HttpClient _http;
    private readonly ILogger<CoinGeckoProvider> _logger;

    public CoinGeckoProvider(HttpClient http, ILogger<CoinGeckoProvider> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<CryptoItemDto>> GetAsync(IEnumerable<string> ids, CancellationToken ct = default)
    {
        var idsCsv = string.Join(",", ids ?? Enumerable.Empty<string>());
        if (string.IsNullOrWhiteSpace(idsCsv))
            return new List<CryptoItemDto>();

        // price_change_percentage را حتماً ست کن تا 7d/30d/200d بیاد
        var url =
            $"https://api.coingecko.com/api/v3/coins/markets" +
            $"?vs_currency=usd&ids={Uri.EscapeDataString(idsCsv)}" +
            $"&price_change_percentage=24h,7d,30d,200d,1y&per_page=250";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        using var res = await _http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        var raw = await JsonSerializer.DeserializeAsync<List<CoinGeckoMarketDto>>(stream, _json, ct)
                  ?? new List<CoinGeckoMarketDto>();

        // مپ دقیق به DTO خودت + تبدیل double -> decimal
        return raw.Select(m => new CryptoItemDto(
            Id: m.id ?? "",
            Symbol: m.symbol ?? "",
            Name: m.name ?? "",
            Image: m.image ?? "",
            CurrentPrice: ToDecimal(m.current_price),
            PriceChangePct7d: ToNullableDecimal(m.price_change_percentage_7d_in_currency),
            PriceChangePct30d: ToNullableDecimal(m.price_change_percentage_30d_in_currency),
            PriceChangePct200d: ToNullableDecimal(m.price_change_percentage_200d_in_currency),
            MarketCap: ToDecimal(m.market_cap),
            TotalVolume: ToDecimal(m.total_volume),
            LastUpdated: m.last_updated?.ToLocalTime() ?? DateTime.UtcNow
        )).ToList();
    }

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // دقیقاً نام فیلدها مثل JSON (snake_case) تا نیاز به JsonPropertyName نباشه
    private sealed class CoinGeckoMarketDto
    {
        public string? id { get; set; }
        public string? symbol { get; set; }
        public string? name { get; set; }
        public string? image { get; set; }
        public double? current_price { get; set; }
        public double? market_cap { get; set; }
        public double? total_volume { get; set; }
        public double? price_change_percentage_24h { get; set; }
        public double? price_change_percentage_7d_in_currency { get; set; }
        public double? price_change_percentage_30d_in_currency { get; set; }
        public double? price_change_percentage_200d_in_currency { get; set; }
        public DateTime? last_updated { get; set; }
    }

    private static decimal ToDecimal(double? v) => v.HasValue ? Convert.ToDecimal(v.Value) : 0m;
    private static decimal? ToNullableDecimal(double? v) => v.HasValue ? Convert.ToDecimal(v.Value) : (decimal?)null;
}