using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.Metals;

public sealed class MetalQuoteDto
{
    public string Symbol { get; set; } = string.Empty; // e.g., XAU
    public decimal Price { get; set; }
    public string QuoteCurrency { get; set; } = "USD";
    public DateTime TimestampUtc { get; set; }
}
