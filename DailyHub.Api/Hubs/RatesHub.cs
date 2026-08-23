using DailyHub.Shared.Abstractions.Rates;
using DailyHub.Shared.Dto.Rates;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace DailyHub.Api.Hubs;

/// <summary>
/// استریم نرخ ارز از Frankfurter (ECB)
/// </summary>
public sealed class RatesHub(IRatesProvider provider, ILogger<RatesHub> logger) : Hub
{
    private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private sealed record CacheEntry(List<FxQuoteDto> Items, DateTime ExpiresAtUtc);

    public async IAsyncEnumerable<List<FxQuoteDto>> Subscribe(
        string baseCurrency = "USD",
        string symbolsCsv = "EUR,GBP,JPY,CHF,CAD,AUD,TRY,CNY,INR",
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var symbols = symbolsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var interval = TimeSpan.FromMinutes(30);

        List<FxQuoteDto> first;
        try
        {
            first = await GetOrFetchAsync(baseCurrency, symbols, ct);
        }
        catch (OperationCanceledException) { yield break; }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Rates.Subscribe initial fetch failed for {Base}", baseCurrency);
            yield break;
        }

        yield return first;

        while (!ct.IsCancellationRequested)
        {
            try { await Task.Delay(interval, ct); }
            catch (OperationCanceledException) { yield break; }

            List<FxQuoteDto>? refreshed = null;
            try { refreshed = await GetOrFetchAsync(baseCurrency, symbols, ct); }
            catch (OperationCanceledException) { yield break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Rates.Subscribe refresh failed for {Base}", baseCurrency);
                continue;
            }

            if (refreshed is not null) yield return refreshed;
        }
    }

    public Task<List<FxQuoteDto>> RefreshOnce(string baseCurrency = "USD", string symbolsCsv = "EUR,GBP,JPY,CHF,CAD,AUD,TRY,CNY,INR")
    {
        var symbols = symbolsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(Context.ConnectionAborted, timeoutCts.Token);
        return GetOrFetchAsync(baseCurrency, symbols, linked.Token, force: true);
    }

    private async Task<List<FxQuoteDto>> GetOrFetchAsync(string @base, string[] symbols, CancellationToken ct, bool force = false)
    {
        var key = $"{@base.ToUpperInvariant()}|{string.Join(",", symbols.OrderBy(s => s))}";
        CacheEntry? hit = null;

        if (!force && _cache.TryGetValue(key, out hit) && hit.ExpiresAtUtc > DateTime.UtcNow)
            return hit.Items;

        var items = (await provider.GetRatesAsync(@base, symbols, ct)).ToList();
        if (items.Count == 0 && hit is not null)
            return hit.Items;

        // فرانکفورتر روزانه آپدیت می‌شود؛ ۳۰ دقیقه کش کافی است
        _cache[key] = new CacheEntry(items, DateTime.UtcNow.AddMinutes(30));
        return items;
    }
}
