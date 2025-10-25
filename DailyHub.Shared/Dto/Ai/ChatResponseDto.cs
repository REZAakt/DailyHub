using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.Ai;

public sealed class ChatRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
}

public sealed class ChatResponseDto
{
    public string Response { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
}
