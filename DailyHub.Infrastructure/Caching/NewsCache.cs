using DailyHub.Shared.Abstractions.News;
using DailyHub.Shared.Dto.News;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Caching
{

    public sealed class NewsCache(IMemoryCache cache, INewsProvider provider) : INewsCache
    {
        public async Task<List<NewsItemDto>> GetOrFetchAsync(string category, TimeSpan maxAge, CancellationToken ct = default)
        {
            var key = $"news:{category}";
            if (cache.TryGetValue(key, out List<NewsItemDto> fresh)) return fresh;

            try
            {
                var items = await provider.GetNewsAsync(category, ct);
                cache.Set(key, items, maxAge);
                return items;
            }
            catch
            {
                if (cache.TryGetValue(key, out List<NewsItemDto> stale)) return stale; // آفلاین: آخرین داده
                throw;
            }
        }
    }

}
