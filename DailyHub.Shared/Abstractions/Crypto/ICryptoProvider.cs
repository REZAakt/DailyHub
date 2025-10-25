using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Abstractions.Crypto;

using DailyHub.Shared.Dto.Crypto;

public interface ICryptoProvider
{
    Task<IReadOnlyList<CryptoQuoteDto>> GetPricesAsync(IEnumerable<string> symbols, CancellationToken ct = default);
}
