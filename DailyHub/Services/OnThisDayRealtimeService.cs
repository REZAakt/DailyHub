using DailyHub.Shared.Dto.Otd;
using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

public sealed class OnThisDayRealtimeService : IAsyncDisposable
{
    private HubConnection? _hub;
    private CancellationTokenSource? _streamCts;
    private Task? _streamTask;

    public event Action<string, List<OnThisDayItemDto>>? OnOtdUpdated;

    //private const string ApiBaseUrl = "https://server2.app.sanaerp.ir";
    private const string ApiBaseUrl = "https://localhost:7014";

    private static readonly string HubUrl = $"{ApiBaseUrl}/hubs/otd";

    public async Task StartAsync()
    {
        if (_hub is not null && _hub.State != HubConnectionState.Disconnected) return;

        _hub = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        await _hub.StartAsync();
    }

    /// <summary>
    /// سابسکرایب با امکان آپدیتِ خودکار شبانه (autoRefresh)
    /// </summary>
    public async Task SubscribeAsync(
        string language, string type, int month, int day,
        bool autoRefresh = false,
        CancellationToken ct = default)
    {
        if (_hub is null) await StartAsync();

        // استریم قبلی رو قطع کن که دوتا لوپ نداشته باشیم
        _streamCts?.Cancel();
        _streamCts?.Dispose();
        _streamCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var stream = _hub!.StreamAsync<List<OnThisDayItemDto>>(
            "Subscribe", language, type, month, day, autoRefresh);

        // لوپ خواندن استریم در پس‌زمینه
        _streamTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var batch in stream.WithCancellation(_streamCts.Token))
                {
                    OnOtdUpdated?.Invoke(type, batch ?? new());
                }
            }
            catch (OperationCanceledException) { /* ignore */ }
        }, _streamCts.Token);
    }

    /// <summary>
    /// رفرش دستی (برای دکمه "بروزرسانی")
    /// </summary>
    public async Task RefreshOnceAsync(string language, string type, int month, int day, CancellationToken ct = default)
    {
        if (_hub is null || _hub.State != HubConnectionState.Connected)
            await StartAsync();

        try
        {
            // name باید دقیقاً با اسم متد هاب یکی باشه
            var items = await _hub!.InvokeAsync<List<OnThisDayItemDto>>(
         "RefreshOnce", language, type, month, day, cancellationToken: ct);

            // خودمون رویداد UI رو فایر می‌کنیم تا صفحه آپدیت شود
            OnOtdUpdated?.Invoke(type, items ?? new());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[OTD RefreshOnceAsync] {ex.GetType().Name}: {ex.Message}");
            // bubble up برای اینکه UI وارد catch خودش شود
            throw;
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

// اگر متد Ignore نداری، حذفش کن یا این اکستنشن کوچیک رو اضافه کن:
internal static class TaskExt { public static void Ignore(this Task? t) { } }
