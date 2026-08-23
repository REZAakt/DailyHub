using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.Crypto
{
    public record CryptoItemDto(
        string Id,
        string Symbol,
        string Name,
        string Image,
        decimal CurrentPrice,
        decimal? PriceChangePct7d,
        decimal? PriceChangePct30d,
        decimal? PriceChangePct200d,
        decimal MarketCap,
        decimal TotalVolume,
        DateTime LastUpdated
    );
}
