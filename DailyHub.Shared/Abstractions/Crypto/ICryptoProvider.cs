using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DailyHub.Shared.Dto.Crypto;

namespace DailyHub.Shared.Abstractions.Crypto;

public interface ICryptoProvider
{
    Task<List<CryptoItemDto>> GetAsync(IEnumerable<string> ids, CancellationToken ct = default);
}