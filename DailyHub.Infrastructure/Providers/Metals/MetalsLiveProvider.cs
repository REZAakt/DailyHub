using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Providers.Metals;

using DailyHub.Shared.Abstractions.Metals;
using DailyHub.Shared.Dto.Metals;

public sealed class MetalsLiveProvider : IMetalsProvider
{
    public Task<IReadOnlyList<MetalQuoteDto>> GetSpotsAsync(IEnumerable<string> symbols, CancellationToken ct = default)
        => throw new NotImplementedException();
}
