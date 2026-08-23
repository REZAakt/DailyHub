using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DailyHub.Shared.Abstractions.OnThisDay;
using DailyHub.Shared.Dto.Otd;
using Microsoft.AspNetCore.SignalR;

public sealed class OnThisDayHub(IOnThisDayProvider provider, ILogger<OnThisDayHub> logger) : Hub
{
    // کش ساده‌ی در-حافظه تا انتهای روز (کلید: زبان+نوع+ماه/روز)
    private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    private sealed record CacheEntry(List<OnThisDayItemDto> Items, DateTime ExpiresAtUtc);

    private static string Key(string lang, string type, int m, int d)
        => $"{lang.Trim().ToLowerInvariant()}|{type.Trim().ToLowerInvariant()}|{m:00}|{d:00}";

    private static DateTime TodayEndUtc()
    {
        var now = DateTime.UtcNow;
        // تا نیمه‌شبِ روزِ بعد (UTC) کش معتبر باشد
        return now.Date.AddDays(1);
    }

    // استریم: یک‌بار + (اختیاری) آپدیت شبانه
    public async IAsyncEnumerable<List<OnThisDayItemDto>> Subscribe(
        string language, string type, int month, int day,
        bool autoRefresh = false,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // 1) از کش بخوان؛ اگر نبود/منقضی بود، بگیر با ریترا‌ی یک‌باره
        var first = await GetOrFetchWithRetry(language, type, month, day, ct);
        yield return first;

        if (!autoRefresh) yield break;

        while (!ct.IsCancellationRequested)
        {
            var delay = DelayUntilNextRun(TimeSpan.FromMinutes(5)); // پنج دقیقه بعد از نیمه‌شب
            try { await Task.Delay(delay, ct); }
            catch (OperationCanceledException) { yield break; }

            var refreshed = await FetchAndUpdateCache(language, type, month, day, ct);
            yield return refreshed;
        }
    }

    // برای دکمه «بروزرسانی»: ریکوئست جدید + به‌روزرسانی کش
    //public Task<List<OnThisDayItemDto>> RefreshOnce(string language, string type, int month, int day)
    //    => FetchAndUpdateCache(language, type, month, day, Context.ConnectionAborted);
    public async Task<List<OnThisDayItemDto>> RefreshOnce(string language, string type, int month, int day)
    {
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(Context.ConnectionAborted, timeoutCts.Token);
        return await FetchAndUpdateCache(language, type, month, day, linked.Token);
    }

    // ---------- داخلی‌ها ----------

    private async Task<List<OnThisDayItemDto>> GetOrFetchWithRetry(
        string language, string type, int month, int day, CancellationToken ct)
    {
        var k = Key(language, type, month, day);

        if (_cache.TryGetValue(k, out var ce) && ce.ExpiresAtUtc > DateTime.UtcNow)
            return ce.Items;

        // اولین تلاش
        var items = await TryFetch(language, type, month, day, ct);
        if (items.Count == 0)
        {
            // یک‌بار دیگر تلاش (ریترا‌ی)
            items = await TryFetch(language, type, month, day, ct);
        }

        // اگر باز هم خالی بود، کش قبلی (اگه هست) را برگردان؛ وگرنه لیست خالی
        if (items.Count == 0 && ce is not null)
            return ce.Items;

        // کش را به‌روز کن
        _cache[k] = new CacheEntry(items, TodayEndUtc());
        return items;
    }

    private async Task<List<OnThisDayItemDto>> FetchAndUpdateCache(
        string language, string type, int month, int day, CancellationToken ct)
    {
        var k = Key(language, type, month, day);

        var items = await TryFetch(language, type, month, day, ct);
        if (items.Count == 0)
        {
            // یک ریترا‌ی
            items = await TryFetch(language, type, month, day, ct);
        }

        if (items.Count == 0)
        {
            // اگر شکست خورد، کش قبلی را نگه دار
            if (_cache.TryGetValue(k, out var ce) && ce.ExpiresAtUtc > DateTime.UtcNow)
                return ce.Items;

            return new(); // هیچ چیز نداریم
        }

        _cache[k] = new CacheEntry(items, TodayEndUtc());
        return items;
    }

    private async Task<List<OnThisDayItemDto>> TryFetch(
        string language, string type, int month, int day, CancellationToken ct)
    {
        try
        {
            return await provider.GetAsync(language, type, month, day, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OTD fetch failed {lang}/{type}/{m}/{d}", language, type, month, day);
            return new();
        }
    }

    private static TimeSpan DelayUntilNextRun(TimeSpan offsetAfterMidnight)
    {
        var now = DateTime.Now;
        var next = now.Date.AddDays(1).Add(offsetAfterMidnight);
        var d = next - now;
        return d > TimeSpan.Zero ? d : TimeSpan.FromHours(24);
    }
}
