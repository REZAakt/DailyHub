using DailyHub.Shared.Dto.Calendar;
using Microsoft.AspNetCore.SignalR.Client;

namespace DailyHub.Client.Services;

public sealed class CalendarRealtimeService : IAsyncDisposable
{
    private HubConnection? _hub;

    public event Action<string, int, List<HolidayDto>>? OnHolidaysUpdated;

    //private const string ApiBaseUrl = "https://server2.app.sanaerp.ir";
    private const string ApiBaseUrl = "https://localhost:7014";

    private static readonly string HubUrl = $"{ApiBaseUrl}/hubs/calendar";

    // داده سالانه است؛ کش کلاینت تا آخر جلسه کافی است
    private readonly Dictionary<string, List<HolidayDto>> _cache = new(StringComparer.OrdinalIgnoreCase);

    public async Task StartAsync()
    {
        if (_hub is not null) return;

        _hub = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        await _hub.StartAsync();
    }

    public async Task<List<HolidayDto>> GetHolidaysAsync(string countryCode, int year, CancellationToken ct = default)
    {
        if (_hub is null) throw new InvalidOperationException("Call StartAsync first.");

        var cc = countryCode.Trim().ToUpperInvariant();
        var key = $"{cc}|{year}";

        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var items = await _hub.InvokeAsync<List<HolidayDto>>("Subscribe", cc, year, ct) ?? new();
        _cache[key] = items;

        OnHolidaysUpdated?.Invoke(cc, year, items);
        return items;
    }

    public async Task<List<HolidayDto>> RefreshOnceAsync(string countryCode, int year, CancellationToken ct = default)
    {
        if (_hub is null || _hub.State != HubConnectionState.Connected)
            await StartAsync();

        var cc = countryCode.Trim().ToUpperInvariant();
        var items = await _hub!.InvokeAsync<List<HolidayDto>>("RefreshOnce", cc, year, ct) ?? new();
        _cache[$"{cc}|{year}"] = items;

        OnHolidaysUpdated?.Invoke(cc, year, items);
        return items;
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
