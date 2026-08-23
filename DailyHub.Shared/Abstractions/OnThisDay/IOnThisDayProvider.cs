using DailyHub.Shared.Dto.Otd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Abstractions.OnThisDay
{
    public interface IOnThisDayProvider
    {
        Task<List<OnThisDayItemDto>> GetAsync(
            string language, string type, int month, int day, CancellationToken ct = default);
    }
}
