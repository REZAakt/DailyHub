using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Abstractions.Calendar;

using DailyHub.Shared.Dto.Calendar;

public interface ICalendarProvider
{
    Task<IReadOnlyList<HolidayDto>> GetHolidaysAsync(string countryCode, int year, CancellationToken ct = default);
}
