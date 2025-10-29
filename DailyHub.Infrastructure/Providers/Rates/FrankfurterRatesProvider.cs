using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Providers.Rates;

using DailyHub.Shared.Dto.Rates;

public sealed class FrankfurterRatesProvider 
{
    public Task<IReadOnlyList<FxQuoteDto>> GetRatesAsync(string baseCurrency, IEnumerable<string> symbols, CancellationToken ct = default)
        => throw new NotImplementedException();
}
