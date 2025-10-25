namespace DailyHub.Api.Endpoints;

using DailyHub.Shared.Abstractions.News;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

public static class NewsEndpoints
{
    public static IEndpointRouteBuilder MapNewsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/news", (string source, int limit, INewsProvider provider) =>
        {
            // Implement later
            throw new NotImplementedException();
        });

        return app;
    }
}
