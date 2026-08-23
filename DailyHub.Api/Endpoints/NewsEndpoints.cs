namespace DailyHub.Api.Endpoints;

using DailyHub.Shared.Abstractions.News;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

public static class NewsEndpoints
{
    public static void MapNewsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/news");

        group.MapGet("/{category}", async (string category, INewsProvider provider) =>
        {
            var news = await provider.GetNewsAsync(category);
            return Results.Ok(news);
        });
    }
}
