using DailyHub.Shared.Abstractions.Crypto;
using DailyHub.Shared.Dto.Crypto;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public sealed class CryptoHub(ICryptoProvider provider, ILogger<CryptoHub> logger) : Hub
{
    public async IAsyncEnumerable<List<CryptoItemDto>> Subscribe(
        string idsCsv,
        bool force = false,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var ids = idsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var interval = TimeSpan.FromMinutes(5);

        // مرحله اول: یک بار دریافت اولیه (خارج از try/yield)
        List<CryptoItemDto> firstData;
        try
        {
            firstData = await provider.GetAsync(ids, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Crypto.Subscribe initial fetch failed for {Ids}", idsCsv);
            yield break;
        }

        // اولین بار yield
        yield return firstData;

        // مرحله دوم: حلقه آپدیت دوره‌ای بدون try خارجی
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(interval, ct);

            List<CryptoItemDto>? refreshed = null;
            bool ok = true;
            try
            {
                refreshed = await provider.GetAsync(ids, ct);
            }
            catch (Exception ex)
            {
                ok = false;
                logger.LogWarning(ex, "Crypto.Subscribe refresh failed for {Ids}", idsCsv);
            }

            if (ok && refreshed is not null)
                yield return refreshed;
        }
    }
}
