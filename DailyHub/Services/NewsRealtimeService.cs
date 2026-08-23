using Microsoft.AspNetCore.SignalR.Client;
using DailyHub.Shared.Dto.News;

public sealed class NewsRealtimeService : IAsyncDisposable
{
    private HubConnection? _hub;
    private CancellationTokenSource? _pollCts;

    public event Action<string, List<NewsItemDto>>? OnNewsUpdated;

    //private const string ApiBaseUrl = "https://server2.app.sanaerp.ir"; // پورت API خودت
    private const string ApiBaseUrl = "https://localhost:7014";

    private static readonly string HubUrl = $"{ApiBaseUrl}/hubs/news";

    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(30);

    private readonly Dictionary<string, (List<NewsItemDto> Items, DateTime AsOf)> _cache = new();

    public async Task StartAsync()
    {
        if (_hub is not null) return;

        _hub = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        await _hub.StartAsync();
    }

    public async Task SubscribeAsync(string category, bool force = false, CancellationToken ct = default)
    {
        if (_hub is null) throw new InvalidOperationException("Hub not started");

        if (!force && _cache.TryGetValue(category, out var entry) &&
            (DateTime.UtcNow - entry.AsOf) < RefreshInterval)
        {
            OnNewsUpdated?.Invoke(category, entry.Items); // از کش کلاینت
            return;
        }

        await foreach (var items in _hub.StreamAsync<List<NewsItemDto>>("Subscribe", category, ct))
        {
            _cache[category] = (items, DateTime.UtcNow);
            OnNewsUpdated?.Invoke(category, items);
            break;
        }
    }

    public async Task ForceRefreshAllAsync(IEnumerable<string> categories, CancellationToken ct = default)
    {
        foreach (var c in categories)
            await SubscribeAsync(c, force: true, ct);
    }

    public async ValueTask DisposeAsync()
    {
        try { _pollCts?.Cancel(); } catch { }
        if (_hub is not null) await _hub.DisposeAsync();
    }
}
