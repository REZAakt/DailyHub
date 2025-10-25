using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.Calendar;

public sealed class HolidayDto
{
    public DateTime Date { get; set; }
    public string LocalName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}
