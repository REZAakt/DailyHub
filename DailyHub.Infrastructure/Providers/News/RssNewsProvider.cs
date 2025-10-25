using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Providers.News;

using DailyHub.Shared.Abstractions.News;
using DailyHub.Shared.Dto.News;

public sealed class RssNewsProvider : INewsProvider
{
    public Task<IReadOnlyList<NewsItemDto>> GetLatestAsync(string source, int limit = 10, CancellationToken ct = default)
        => throw new NotImplementedException();
}
