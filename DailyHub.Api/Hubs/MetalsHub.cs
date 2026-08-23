using DailyHub.Shared.Abstractions.Metals;
using DailyHub.Shared.Dto.Metals;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DailyHub.Api.Hubs;

/// <summary>
/// استریم قیمت فلزات گران‌بها (طلا، نقره، پلاتین، پالادیم)
/// </summary>
public sealed class MetalsHub(IMetalsProvider provider, ILogger<MetalsHub> logger) : Hub
{
    private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private sealed record CacheEntry(List<MetalQuoteDto> Items, DateTime ExpiresAtUtc);

    public async IAsyncEnumerable<List<MetalQuoteDto>> Subscribe(
        string symbolsCsv = "XAU,XAG,XPT,XPD",
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var symbols = symbolsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var interval = TimeSpan.FromMinutes(5);

        List<MetalQuoteDto> first;
        try
        {
            first = await GetOrFetchAsync(symbols, TimeSpan.FromMinutes(2), ct);
        }
        catch (OperationCanceledException) { yield break; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Metals.Subscribe initial fetch failed for {Symbols}", symbolsCsv);
            yield break;
        }

        yield return first;

        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(interval, ct); }
            catch (OperationCanceledException) { yield break; }

            List<MetalQuoteDto>? refreshed = null;
            try { refreshed = await GetOrFetchAsync(symbols, TimeSpan.FromMinutes(2), ct); }
            catch (OperationCanceledException) { yield break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Metals.Subscribe refresh failed for {Symbols}", symbolsCsv);
                continue;
            }

            if (refreshed is not null) yield return refreshed;
        }
    }

    // رفرش دستی (دکمه «بروزرسانی»)
    public Task<List<MetalQuoteDto>> RefreshOnce(string symbolsCsv = "XAU,XAG,XPT,XPD")
    {
        var symbols = symbolsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(Context.ConnectionAborted, timeoutCts.Token);
        return GetOrFetchAsync(symbols, TimeSpan.Zero, linked.Token);
    }

    private async Task<List<MetalQuoteDto>> GetOrFetchAsync(string[] symbols, TimeSpan ttl, CancellationToken ct)
    {
        var key = string.Join(",", symbols.OrderBy(s => s)).ToUpperInvariant();
        CacheEntry? hit = null;

        if (ttl > TimeSpan.Zero && _cache.TryGetValue(key, out hit) && hit.ExpiresAtUtc > DateTime.UtcNow)
            return hit.Items;

        var items = (await provider.GetSpotsAsync(symbols, ct)).ToList();
        if (items.Count == 0 && hit is not null)
            return hit.Items; // اگر سرویس خطا داد، کش قبلی را نگه دار

        _cache[key] = new CacheEntry(items, DateTime.UtcNow.Add(ttl > TimeSpan.Zero ? ttl : TimeSpan.FromMinutes(2)));
        return items;
    }
}
