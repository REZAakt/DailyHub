namespace DailyHub.Shared.Abstractions.Chat;

using DailyHub.Shared.Dto.Ai;
using System.Collections.Generic;
using System.Threading;

public interface IDeepseekProvider
{
    // استریم (ترجیحی برای UI چت)
    IAsyncEnumerable<ChatChunkDto> StreamAsync(
        IReadOnlyList<ChatMessageDto> messages,
        ChatSettingsDto settings,
        CancellationToken ct = default
    );

    // غیر استریم (در صورت نیاز)
    Task<string> CompleteAsync(
        IReadOnlyList<ChatMessageDto> messages,
        ChatSettingsDto settings,
        CancellationToken ct = default
    );
}
