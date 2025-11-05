using System.Diagnostics;
using DailyHub.Shared.Dto.Ai;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;

public sealed class DeepseekRealtimeService : IAsyncDisposable
{
    private HubConnection? _hub;
    private const string ApiBaseUrl = "https://localhost:7014";
    private static readonly string HubUrl = $"{ApiBaseUrl}/hubs/deepseek";

    public event Action<string>? OnDelta;        // تکه‌های جدید
    public event Action<string?>? OnCompleted;   // پایان پاسخ (content کامل)
    public event Action<string>? OnError;        // خطا

    public async Task StartAsync()
    {
        if (_hub is not null) return;

        _hub = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        await _hub.StartAsync();
    }

    public async Task StreamAsync(
        List<ChatMessageDto> messages,
        ChatSettingsDto settings,
        CancellationToken ct = default)
    {
        if (_hub is null || _hub.State != HubConnectionState.Connected)
            await StartAsync();

        var sb = new StringBuilder();

        try
        {
            var stream = _hub!.StreamAsync<ChatChunkDto>("StreamChat", messages, settings, ct);
            await foreach (var chunk in stream.WithCancellation(ct))
            {
                if (!string.IsNullOrEmpty(chunk.Error))
                {
                    OnError?.Invoke(chunk.Error);
                    break;
                }
                if (!string.IsNullOrEmpty(chunk.Delta))
                {
                    sb.Append(chunk.Delta);
                    OnDelta?.Invoke(chunk.Delta);
                }
                if (chunk.IsFinal)
                {
                    OnCompleted?.Invoke(sb.ToString());
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Deepseek StreamAsync] {ex.GetType().Name}: {ex.Message}");
            OnError?.Invoke("خطا در ارتباط با سرور");
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
