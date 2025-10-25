using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Providers.Crypto;

using DailyHub.Shared.Abstractions.Crypto;
using DailyHub.Shared.Dto.Crypto;

public sealed class CoinDeskProvider : ICryptoProvider
{
    public Task<IReadOnlyList<CryptoQuoteDto>> GetPricesAsync(IEnumerable<string> symbols, CancellationToken ct = default)
        => throw new NotImplementedException();
}
