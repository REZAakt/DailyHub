using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using DailyHub.Shared.Abstractions.News;
using DailyHub.Shared.Dto.News;

public sealed class RssNewsProvider(HttpClient http) : INewsProvider
{
    public async Task<List<NewsItemDto>> GetNewsAsync(string category, CancellationToken ct = default)
    {

        var url = category switch
        {
            "sports" => "https://en.isna.ir/rss/tp/23",   // Sports (پایدار)
            "politics" => "https://en.isna.ir/rss/tp/13",
            "economy" => "https://en.isna.ir/rss/tp/33",   // به‌جای donya-e-eqtesad
            "health" => "https://www.yjc.ir/fa/rss",      // فید کلی YJC
            "all" => "https://www.isna.ir/rss",
            _ => "https://www.isna.ir/rss"
        };


        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5)); // ⏱ فقط ۵ ثانیه صبر کن

            var xml = await http.GetStringAsync(url, cts.Token);
            var doc = XDocument.Parse(xml);

            return doc.Descendants("item").Take(30).Select(x => new NewsItemDto
            {
                Title = x.Element("title")?.Value ?? "",
                Summary = x.Element("description")?.Value ?? "",
                Link = x.Element("link")?.Value ?? "",
                PublishedAt = DateTime.TryParse(x.Element("pubDate")?.Value, out var dt) ? dt : DateTime.UtcNow
            }).ToList();
        }
        catch (Exception ex)
        {
            // 🧠 فید پشتیبان در صورت خطا
            var fallbackXml = await http.GetStringAsync("https://www.isna.ir/rss", ct);
            var doc = XDocument.Parse(fallbackXml);

            return doc.Descendants("item").Take(30).Select(x => new NewsItemDto
            {
                Title = x.Element("title")?.Value ?? "",
                Summary = x.Element("description")?.Value ?? "",
                Link = x.Element("link")?.Value ?? "",
                PublishedAt = DateTime.TryParse(x.Element("pubDate")?.Value, out var dt) ? dt : DateTime.UtcNow
            }).ToList();
        }
    }

}
