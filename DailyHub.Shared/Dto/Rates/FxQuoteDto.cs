using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.Rates;

public sealed class FxQuoteDto
{
    public string Base { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime TimestampUtc { get; set; }
}
