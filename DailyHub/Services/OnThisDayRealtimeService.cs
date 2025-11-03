using DailyHub.Shared.Dto.Otd;
using Microsoft.AspNetCore.SignalR.Client;

public sealed class OnThisDayRealtimeService : IAsyncDisposable
{
    private HubConnection? _hub;

    public event Action<string, List<OnThisDayItemDto>>? OnOtdUpdated;

    private const string ApiBaseUrl = "https://localhost:7014";
    private static readonly string HubUrl = $"{ApiBaseUrl}/hubs/otd";

    public async Task StartAsync()
    {
        if (_hub is not null) return;

        _hub = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        await _hub.StartAsync();
    }

    public async Task SubscribeAsync(string language, string type, int month, int day, CancellationToken ct = default)
    {
        if (_hub is null) await StartAsync();

        var stream = _hub!.StreamAsync<List<OnThisDayItemDto>>("Subscribe", language, type, month, day, ct);
        await foreach (var batch in stream.WithCancellation(ct))
        {
            OnOtdUpdated?.Invoke(type, batch ?? new());
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
