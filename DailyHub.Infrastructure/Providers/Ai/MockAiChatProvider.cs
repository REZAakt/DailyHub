using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Providers.Ai;

using DailyHub.Shared.Abstractions.Ai;
using DailyHub.Shared.Dto.Ai;

public sealed class MockAiChatProvider : IAiChatProvider
{
    public Task<ChatResponseDto> ReplyAsync(ChatRequestDto request, CancellationToken ct = default)
        => throw new NotImplementedException();
}
