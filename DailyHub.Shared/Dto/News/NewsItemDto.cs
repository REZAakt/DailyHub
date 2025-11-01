using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Shared.Dto.News;

public sealed class NewsItemDto
{
    public string Title { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Link { get; set; } = "";
    public DateTime PublishedAt { get; set; }
}
