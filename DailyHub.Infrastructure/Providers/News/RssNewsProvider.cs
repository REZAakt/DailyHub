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
            "sports" => "https://www.varzesh3.com/rss/all",                          // ✅ سالم
            "politics" => "https://www.irna.ir/rss/tp/5",                           // ✅ سالم و RSS واقعی
            "economy" => "https://www.irna.ir/rss/tp/20",                           // ✅ سالم
            "health" => "https://www.irna.ir/rss/tp/1001681",                            // ✅ سالم
            "all" => "https://www.isna.ir/rss",                                   // ✅ سالم
            _ => "https://www.isna.ir/rss"
        };


        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(15)); // ⏱ حداکثر ۶ ثانیه منتظر بمون

            var xml = await http.GetStringAsync(url, cts.Token);
            var doc = XDocument.Parse(xml);

            var items = doc.Descendants("item")
                .Select(x => new NewsItemDto
                {
                    Title = x.Element("title")?.Value?.Trim() ?? "",
                    Summary = x.Element("description")?.Value?.Trim() ?? "",
                    Link = x.Element("link")?.Value?.Trim() ?? "",
                    PublishedAt = DateTime.TryParse(x.Element("pubDate")?.Value, out var dt)
                        ? dt.ToLocalTime()
                        : DateTime.UtcNow
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Title) && !string.IsNullOrWhiteSpace(x.Link))
                .GroupBy(x => x.Link) // 🔹 حذف خبرهای تکراری بر اساس لینک
                .Select(g => g.First())
                .Take(30)
                .ToList();

            return items;
        }
        catch (Exception ex)
        {
            // ⚠️ در صورت خطا (مثلاً فیلترینگ، قطعی اینترنت، یا timeout)
            Console.WriteLine($"[RSS ERROR] {category}: {ex.Message}");

            // ✅ فید پشتیبان از ایسنا
            try
            {
                var fallbackXml = await http.GetStringAsync("https://www.isna.ir/rss/all.xml", ct);
                var doc = XDocument.Parse(fallbackXml);

                var items = doc.Descendants("item")
                    .Select(x => new NewsItemDto
                    {
                        Title = x.Element("title")?.Value?.Trim() ?? "",
                        Summary = x.Element("description")?.Value?.Trim() ?? "",
                        Link = x.Element("link")?.Value?.Trim() ?? "",
                        PublishedAt = DateTime.TryParse(x.Element("pubDate")?.Value, out var dt)
                            ? dt.ToLocalTime()
                            : DateTime.UtcNow
                    })
                    .GroupBy(x => x.Link)
                    .Select(g => g.First())
                    .Take(30)
                    .ToList();

                return items;
            }
            catch
            {
                // اگر حتی فید پشتیبان هم در دسترس نبود، خروجی خالی برگردون
                return new List<NewsItemDto>();
            }
        }
    }
}
