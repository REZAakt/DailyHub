using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Common;

public sealed class PageRequest
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
}

public sealed class PageResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int Size { get; init; }
    public int Total { get; init; }
}
