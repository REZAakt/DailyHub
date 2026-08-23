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

        // 1) گرفتن داده اولیه
        List<CryptoItemDto> firstData;
        try
        {
            firstData = await provider.GetAsync(ids, ct);
        }
        catch (OperationCanceledException)
        {
            // کلاینت رفت، ما هم میریم
            yield break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Crypto.Subscribe initial fetch failed for {Ids}", idsCsv);
            yield break;
        }

        // اولین نتیجه
        yield return firstData;

        // 2) حلقه‌ی بازه‌ای
        while (!ct.IsCancellationRequested)
        {
            // تاخیر با امکان کنسل شدن
            try
            {
                await Task.Delay(interval, ct);
            }
            catch (OperationCanceledException)
            {
                // یعنی استریم بسته شده
                yield break;
            }

            List<CryptoItemDto>? refreshed = null;

            try
            {
                refreshed = await provider.GetAsync(ids, ct);
            }
            catch (OperationCanceledException)
            {
                // باز هم یعنی کلاینت رفت
                yield break;
            }
            catch (Exception ex)
            {
                // اینجا ارور سرویسه، نه کنسل. پس لاگ کن و ادامه بده
                logger.LogWarning(ex, "Crypto.Subscribe refresh failed for {Ids}", idsCsv);
                continue;
            }

            if (refreshed is not null)
            {
                yield return refreshed;
            }
        }
    }
}
