using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Abstractions.News;

using DailyHub.Shared.Dto.News;

//public interface INewsProvider
//{
//    Task<IReadOnlyList<NewsItemDto>> GetLatestAsync(string source, int limit = 10, CancellationToken ct = default);
//}

public interface INewsProvider
{
    Task<List<NewsItemDto>> GetNewsAsync(string category, CancellationToken ct = default);
}
