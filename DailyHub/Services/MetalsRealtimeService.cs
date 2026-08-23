using DailyHub.Shared.Dto.Metals;
using Microsoft.AspNetCore.SignalR.Client;

namespace DailyHub.Client.Services;

public sealed class MetalsRealtimeService : IAsyncDisposable
{
    private HubConnection? _hub;

    public event Action<List<MetalQuoteDto>>? OnMetalsUpdated;

    //private const string ApiBaseUrl = "https://server2.app.sanaerp.ir";
    private const string ApiBaseUrl = "https://localhost:7014";

    private static readonly string HubUrl = $"{ApiBaseUrl}/hubs/metals";

    // کش سمت کلاینت
    private List<MetalQuoteDto>? _lastItems;
    private DateTime _lastFetchUtc;
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(2);

    public async Task StartAsync()
    {
        if (_hub is not null) return;

        _hub = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        await _hub.StartAsync();
    }

    public async Task SubscribeAsync(string symbolsCsv = "XAU,XAG,XPT,XPD", bool force = false, CancellationToken ct = default)
    {
        if (_hub is null) throw new InvalidOperationException("Call StartAsync first.");

        // اگر کش کلاینت تازه است، مستقیم برگردان
        if (!force && _lastItems is not null && (DateTime.UtcNow - _lastFetchUtc) < RefreshInterval)
        {
            OnMetalsUpdated?.Invoke(_lastItems);
            return;
        }

        await foreach (var items in _hub.StreamAsync<List<MetalQuoteDto>>("Subscribe", symbolsCsv, ct))
        {
            _lastItems = items ?? new();
            _lastFetchUtc = DateTime.UtcNow;
            OnMetalsUpdated?.Invoke(_lastItems);
            break; // اولین پاسخ کافی است؛ سرور هر ۵ دقیقه دوباره می‌فرستد
        }
    }

    public async Task RefreshOnceAsync(string symbolsCsv = "XAU,XAG,XPT,XPD", CancellationToken ct = default)
    {
        if (_hub is null || _hub.State != HubConnectionState.Connected)
            await StartAsync();

        var items = await _hub!.InvokeAsync<List<MetalQuoteDto>>("RefreshOnce", symbolsCsv, ct);
        _lastItems = items ?? new();
        _lastFetchUtc = DateTime.UtcNow;
        OnMetalsUpdated?.Invoke(_lastItems);
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
