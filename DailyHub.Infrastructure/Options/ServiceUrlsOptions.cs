using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Options;

public sealed class ServiceUrlsOptions
{
    public const string SectionName = "ServiceUrls";
    public string? Weather { get; init; }
    public string? News { get; init; }
    public string? Rates { get; init; }
    public string? Crypto { get; init; }
    public string? Metals { get; init; }
    public string? Calendar { get; init; }
}
