using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.News;

public sealed class NewsItemDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime PublishedUtc { get; set; }
}
