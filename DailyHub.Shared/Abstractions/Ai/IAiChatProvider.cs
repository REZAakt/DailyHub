using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Abstractions.Ai;

using DailyHub.Shared.Dto.Ai;

public interface IAiChatProvider
{
    Task<ChatResponseDto> ReplyAsync(ChatRequestDto request, CancellationToken ct = default);
}
