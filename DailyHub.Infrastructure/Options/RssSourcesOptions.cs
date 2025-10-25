using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.Options;

public sealed class RssSourcesOptions
{
    public const string SectionName = "RssSources";
    public Dictionary<string, string> Sources { get; init; } = new();
}
