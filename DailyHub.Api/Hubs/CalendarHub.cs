using DailyHub.Shared.Abstractions.Calendar;
using DailyHub.Shared.Dto.Calendar;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace DailyHub.Api.Hubs;

/// <summary>
/// تعطیلات رسمی کشورها (کش تا آخر روز، چون داده سالانه است)
/// </summary>
public sealed class CalendarHub(ICalendarProvider provider, ILogger<CalendarHub> logger) : Hub
{
    private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private sealed record CacheEntry(List<HolidayDto> Items, DateTime ExpiresAtUtc);

    public async Task<List<HolidayDto>> Subscribe(string countryCode, int year)
    {
        var cc = string.IsNullOrWhiteSpace(countryCode) ? "IR" : countryCode.Trim().ToUpperInvariant();
        if (year <= 0) year = DateTime.UtcNow.Year;

        var key = $"{cc}|{year}";
        if (_cache.TryGetValue(key, out var hit) && hit.ExpiresAtUtc > DateTime.UtcNow)
            return hit.Items;

        var items = await FetchAsync(cc, year, Context.ConnectionAborted);
        _cache[key] = new CacheEntry(items, DateTime.UtcNow.AddDays(1));
        return items;
    }

    public Task<List<HolidayDto>> RefreshOnce(string countryCode, int year)
    {
        var cc = string.IsNullOrWhiteSpace(countryCode) ? "IR" : countryCode.Trim().ToUpperInvariant();
        if (year <= 0) year = DateTime.UtcNow.Year;

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(Context.ConnectionAborted, timeoutCts.Token);
        return FetchAsync(cc, year, linked.Token, force: true);
    }

    private async Task<List<HolidayDto>> FetchAsync(string cc, int year, CancellationToken ct, bool force = false)
    {
        var key = $"{cc}|{year}";
        try
        {
            var items = (await provider.GetHolidaysAsync(cc, year, ct)).ToList();
            if (items.Count == 0 && !force && _cache.TryGetValue(key, out var old))
                return old.Items;

            _cache[key] = new CacheEntry(items, DateTime.UtcNow.AddDays(1));
            return items;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Calendar fetch failed for {Country}/{Year}", cc, year);
            if (_cache.TryGetValue(key, out var old)) return old.Items;
            return new List<HolidayDto>();
        }
    }
}
