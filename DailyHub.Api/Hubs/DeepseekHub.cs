using System.Runtime.CompilerServices;
using DailyHub.Shared.Abstractions.Chat;
using DailyHub.Shared.Dto.Ai;
using Microsoft.AspNetCore.SignalR;

public sealed class DeepseekHub(IDeepseekProvider provider, ILogger<DeepseekHub> logger) : Hub
{
    // استریم پاسخ
    public async IAsyncEnumerable<ChatChunkDto> StreamChat(
        List<ChatMessageDto> messages,
        ChatSettingsDto settings,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        IAsyncEnumerable<ChatChunkDto>? stream = null;
        bool initFailed = false;
        string? initError = null;

        try
        {
            stream = provider.StreamAsync(messages, settings, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Deepseek StreamChat init failed");
            initFailed = true;
            initError = "Init failed";
        }

        // ⬇️ خارج از try/catch yield کن تا CS1631 نگیری
        if (initFailed)
        {
            yield return new ChatChunkDto(null, true, initError);
            yield break;
        }

        // استریم را فوروارد کن
        await foreach (var chunk in stream!.WithCancellation(ct))
        {
            yield return chunk;
        }
    }

    // تک‌پاسخی (غیر استریم)
    public Task<string> CompleteOnce(
        List<ChatMessageDto> messages,
        ChatSettingsDto settings,
        CancellationToken ct = default)
        => provider.CompleteAsync(messages, settings, ct);
}
