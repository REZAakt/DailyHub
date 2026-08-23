using DailyHub.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.Ai;

//public sealed class ChatRequestDto
//{
//    public string Message { get; set; } = string.Empty;
//    public string? ConversationId { get; set; }
//}

//public sealed class ChatResponseDto
//{
//    public string Response { get; set; } = string.Empty;
//    public string? ConversationId { get; set; }
//}


public record ChatMessageDto(ChatRole Role, string Content);

public record ChatSettingsDto(
    string Model = "deepseek-chat",
    int MaxTokens = 512,
    double Temperature = 0.7
);

// برای استریم
public record ChatChunkDto(
    string? Delta,        // تکه متن جدید
    bool IsFinal,         // بسته شدن پاسخ
    string? Error = null  // پیام خطا (اگر بود)
);