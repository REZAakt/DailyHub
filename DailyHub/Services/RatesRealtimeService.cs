using DailyHub.Shared.Dto.Rates;
using Microsoft.AspNetCore.SignalR.Client;

namespace DailyHub.Client.Services;

public sealed class RatesRealtimeService : IAsyncDisposable
{
    private HubConnection? _hub;

    public event Action<string, List<FxQuoteDto>>? OnRatesUpdated;

    //private const string ApiBaseUrl = "https://server2.app.sanaerp.ir";
    private const string ApiBaseUrl = "https://localhost:7014";

    private static readonly string HubUrl = $"{ApiBaseUrl}/hubs/rates";

    private readonly Dictionary<string, (List<FxQuoteDto> Items, DateTime AsOf)> _cache = new();
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(30);

    public async Task StartAsync()
    {
        if (_hub is not null) return;

        _hub = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        await _hub.StartAsync();
    }

    public async Task SubscribeAsync(string baseCurrency = "USD", string symbolsCsv = "EUR,GBP,JPY,CHF,CAD,AUD,TRY,CNY,INR",
        bool force = false, CancellationToken ct = default)
    {
        if (_hub is null) throw new InvalidOperationException("Call StartAsync first.");

        if (!force && _cache.TryGetValue(baseCurrency, out var hit) && (DateTime.UtcNow - hit.AsOf) < RefreshInterval)
        {
            OnRatesUpdated?.Invoke(baseCurrency, hit.Items);
            return;
        }

        await foreach (var items in _hub.StreamAsync<List<FxQuoteDto>>("Subscribe", baseCurrency, symbolsCsv, ct))
        {
            _cache[baseCurrency] = (items ?? new(), DateTime.UtcNow);
            OnRatesUpdated?.Invoke(baseCurrency, items ?? new());
            break;
        }
    }

    public async Task RefreshOnceAsync(string baseCurrency = "USD", string symbolsCsv = "EUR,GBP,JPY,CHF,CAD,AUD,TRY,CNY,INR",
        CancellationToken ct = default)
    {
        if (_hub is null || _hub.State != HubConnectionState.Connected)
            await StartAsync();

        var items = await _hub!.InvokeAsync<List<FxQuoteDto>>("RefreshOnce", baseCurrency, symbolsCsv, ct);
        _cache[baseCurrency] = (items ?? new(), DateTime.UtcNow);
        OnRatesUpdated?.Invoke(baseCurrency, items ?? new());
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null)
        {
            await _hub.DisposeAsync();
            _hub = null;
        }
    }
}
