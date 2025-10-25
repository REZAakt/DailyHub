namespace DailyHub.Api.Extensions;

using DailyHub.Api.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapDailyHubEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapWeatherEndpoints();
        app.MapNewsEndpoints();
        app.MapCalendarEndpoints();
        app.MapCryptoEndpoints();
        app.MapAiChatEndpoints();
        // Metals / Rates intentionally omitted for now
        return app;
    }
}
