using DailyHub.Shared.Dto.Crypto;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

public sealed class CryptoRealtimeService : IAsyncDisposable
{
    private HubConnection? _hub;

    public event Action<string, List<CryptoItemDto>>? OnCryptoUpdated;

    private const string ApiBaseUrl = "https://server2.app.sanaerp.ir";          // همون Base سرور
    private static readonly string HubUrl = $"{ApiBaseUrl}/hubs/crypto";

    // کش سمت کلاینت (اختیاری)
    private readonly Dictionary<string, (List<CryptoItemDto> Items, DateTime AsOf)> _cache = new();
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

    public async Task SubscribeAsync(string idsCsv, bool force = false, CancellationToken ct = default)
    {
        if (_hub is null) throw new InvalidOperationException("Call StartAsync first.");

        // اگر در کش تازه داریم، مستقیم برگردونیم
        if (!force && _cache.TryGetValue(idsCsv, out var hit) && (DateTime.UtcNow - hit.AsOf) < RefreshInterval)
        {
            OnCryptoUpdated?.Invoke(idsCsv, hit.Items);
            return;
        }

        await foreach (var list in _hub.StreamAsync<List<CryptoItemDto>>("Subscribe", idsCsv, force, ct))
        {
            _cache[idsCsv] = (list ?? new(), DateTime.UtcNow);
            OnCryptoUpdated?.Invoke(idsCsv, list ?? new());
        }
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
