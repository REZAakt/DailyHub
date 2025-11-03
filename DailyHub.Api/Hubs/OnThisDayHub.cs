using System.Runtime.CompilerServices;
using DailyHub.Shared.Abstractions.OnThisDay;
using DailyHub.Shared.Dto.Otd;
using Microsoft.AspNetCore.SignalR;

public sealed class OnThisDayHub(IOnThisDayProvider provider, ILogger<OnThisDayHub> logger) : Hub
{
    // stream: یک‌بار ارسال + آپدیت دوره‌ای اختیاری
    public async IAsyncEnumerable<List<OnThisDayItemDto>> Subscribe(
        string language, string type, int month, int day,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        List<OnThisDayItemDto> first = [];
        try
        {
            first = await provider.GetAsync(language, type, month, day, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OTD.Subscribe initial fetch failed {lang}/{type}/{m}/{d}",
                language, type, month, day);
        }
        yield return first;

        // اگر خواستی هر 6 ساعت آپدیت بده:
        // var interval = TimeSpan.FromHours(6);
        // while (!ct.IsCancellationRequested)
        // {
        //     await Task.Delay(interval, ct);
        //     List<OnThisDayItemDto> refreshed = [];
        //     try { refreshed = await provider.GetAsync(language, type, month, day, ct); }
        //     catch (Exception ex) { logger.LogWarning(ex, "OTD refresh failed"); }
        //     yield return refreshed;
        // }
    }
}
