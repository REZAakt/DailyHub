using Microsoft.AspNetCore.SignalR.Client;
using DailyHub.Shared.Dto.Weather;

namespace DailyHub.Client.Services;

public sealed class WeatherRealtimeService : IAsyncDisposable
{
    private HubConnection? _hub;
    public event Action<WeatherForecastDto>? OnWeatherUpdated;

    private const string ApiBaseUrl = "https://localhost:7014";
    private static readonly string HubUrl = $"{ApiBaseUrl}/hubs/weather";

    public async Task StartAsync()
    {
        if (_hub is not null) return;

        _hub = new HubConnectionBuilder()
           .WithUrl(HubUrl)
           .WithAutomaticReconnect()
           .Build();

        _hub.On<WeatherForecastDto>("WeatherUpdated", dto => OnWeatherUpdated?.Invoke(dto));
        _hub.On<string>("WeatherError", msg => Console.Error.WriteLine($"WeatherError: {msg}"));

        await _hub.StartAsync();
    }

    private CancellationTokenSource? _cts;

    public async Task SubscribeAsync(double lat, double lon, int days = 7)
    {
        if (_hub is null) throw new InvalidOperationException("Hub not started");

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        await foreach (var dto in _hub.StreamAsync<WeatherForecastDto>("Subscribe", lat, lon, days, _cts.Token))
            OnWeatherUpdated?.Invoke(dto);
    }


    public async ValueTask DisposeAsync()
    {
        if (_hub is not null) await _hub.DisposeAsync();
    }
}
