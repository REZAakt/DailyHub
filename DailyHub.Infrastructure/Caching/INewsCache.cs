using DailyHub.Shared.Dto.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Caching
{
    public interface INewsCache
    {
        Task<List<NewsItemDto>> GetOrFetchAsync(string category, TimeSpan maxAge, CancellationToken ct = default);
    }
}
