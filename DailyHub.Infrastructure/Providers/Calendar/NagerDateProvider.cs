using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Providers.Calendar;

using DailyHub.Shared.Abstractions.Calendar;
using DailyHub.Shared.Dto.Calendar;

public sealed class NagerDateProvider : ICalendarProvider
{
    public Task<IReadOnlyList<HolidayDto>> GetHolidaysAsync(string countryCode, int year, CancellationToken ct = default)
        => throw new NotImplementedException();
}
