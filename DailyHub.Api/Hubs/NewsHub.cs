using DailyHub.Infrastructure.Caching;
using DailyHub.Shared.Abstractions.News;
using DailyHub.Shared.Dto.News;
using Microsoft.AspNetCore.SignalR;

public sealed class NewsHub : Hub
{
    private readonly INewsCache _cache;
    private readonly ILogger<NewsHub> _logger;

    public NewsHub(INewsCache cache, ILogger<NewsHub> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async IAsyncEnumerable<List<NewsItemDto>> Subscribe(
        string category,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        List<NewsItemDto>? items = null;

        try
        {
            items = await _cache.GetOrFetchAsync(category, TimeSpan.FromMinutes(30), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "News.Subscribe failed for {Category}", category);
            throw; // بفرستش تا کلاینت بفهمه خطا بوده
        }

        if (items != null)
            yield return items;
    }
}
