namespace DailyHub.Api.Endpoints;

using DailyHub.Shared.Abstractions.Ai;
using DailyHub.Shared.Dto.Ai;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

public static class AiChatEndpoints
{
    public static IEndpointRouteBuilder MapAiChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/ai/chat", (ChatRequestDto req, IAiChatProvider provider) =>
        {
            // Implement later
            throw new NotImplementedException();
        });

        return app;
    }
}
