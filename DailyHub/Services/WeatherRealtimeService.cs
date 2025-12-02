using Microsoft.AspNetCore.SignalR.Client;
using DailyHub.Shared.Dto.Weather;

namespace DailyHub.Client.Services;

public sealed class WeatherRealtimeService : IAsyncDisposable
{
    private HubConnection? _hub;
    private CancellationTokenSource? _pollCts;

    public event Action<WeatherForecastDto>? OnWeatherUpdated;

    // آدرس API را با پورت واقعی خودت یکی کن
    private const string ApiBaseUrl = "https://server2.app.sanaerp.ir";
    private static readonly string HubUrl = $"{ApiBaseUrl}/hubs/weather";

    // کش سمت کلاینت
    private WeatherForecastDto? _lastDto;
    private DateTime _lastFetchUtc;
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(30);

    public async Task StartAsync()
    {
        if (_hub is not null) return;

        _hub = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        await _hub.StartAsync();
    }

    public async Task SubscribeAsync(double lat, double lon, int days = 7, TimeSpan? minFresh = null)
    {
        if (_hub is null) throw new InvalidOperationException("Hub not started");

        var freshness = minFresh ?? RefreshInterval;

        // اگر کش کلاینت تازه است، همونو بده (بدون ریکوئست)
        if (_lastDto is not null && (DateTime.UtcNow - _lastFetchUtc) < freshness)
        {
            OnWeatherUpdated?.Invoke(_lastDto);
        }
        else
        {
            // یک‌بار از هاب بگیر
            await foreach (var dto in _hub.StreamAsync<WeatherForecastDto>("Subscribe", lat, lon, days))
            {
                _lastDto = dto;
                _lastFetchUtc = DateTime.UtcNow;
                OnWeatherUpdated?.Invoke(dto);
                break; // استریم تک‌ آیتمی
            }
        }

        // پولینگ دوره‌ای (هر 30 دقیقه) - قبلی را کنسل و جدید را شروع کن
        _pollCts?.Cancel();
        _pollCts = new CancellationTokenSource();
        _ = PollLoopAsync(lat, lon, days, _pollCts.Token);
    }

    private async Task PollLoopAsync(double lat, double lon, int days, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(RefreshInterval, ct);
                if (ct.IsCancellationRequested) break;

                await foreach (var dto in _hub!.StreamAsync<WeatherForecastDto>("Subscribe", lat, lon, days, ct))
                {
                    _lastDto = dto;
                    _lastFetchUtc = DateTime.UtcNow;
                    OnWeatherUpdated?.Invoke(dto);
                    break;
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Weather poll error: {ex.Message}");
                // 10 ثانیه صبر کن و دوباره تلاش کن
                try { await Task.Delay(TimeSpan.FromSeconds(10), ct); } catch { }
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        try { _pollCts?.Cancel(); } catch { }
        if (_hub is not null) await _hub.DisposeAsync();
    }
}
