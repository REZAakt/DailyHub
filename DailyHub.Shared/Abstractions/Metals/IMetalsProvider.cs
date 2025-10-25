using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Abstractions.Metals;

using DailyHub.Shared.Dto.Metals;

public interface IMetalsProvider
{
    Task<IReadOnlyList<MetalQuoteDto>> GetSpotsAsync(IEnumerable<string> symbols, CancellationToken ct = default);
}
