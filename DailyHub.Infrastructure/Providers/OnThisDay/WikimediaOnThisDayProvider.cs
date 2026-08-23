using System.Text.Json;
using DailyHub.Shared.Abstractions.OnThisDay;
using DailyHub.Shared.Dto.Otd;

namespace DailyHub.Infrastructure.Providers.Otd
{
    public sealed class WikimediaOnThisDayProvider(HttpClient http) : IOnThisDayProvider
    {
        public async Task<List<OnThisDayItemDto>> GetAsync(
            string language, string type, int month, int day, CancellationToken ct = default)
        {
            // اول تلاش با زبان درخواستی (fa)
            var items = await Fetch(language, type, month, day, ct);

            // اگر خالی بود و زبان انگلیسی نبود، دوباره با en بخوان
            if (items.Count == 0 && !string.Equals(language, "en", StringComparison.OrdinalIgnoreCase))
            {
                var enItems = await Fetch("en", type, month, day, ct);
                return enItems;
            }

            return items;
        }

        private async Task<List<OnThisDayItemDto>> Fetch(
            string language, string type, int month, int day, CancellationToken ct)
        {
            var url = $"feed/v1/wikipedia/{language}/onthisday/{type}/{month:00}/{day:00}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.TryAddWithoutValidation("User-Agent", "DailyHub/1.0 (contact: you@example.com)");

            using var res = await http.SendAsync(req, ct);
            res.EnsureSuccessStatusCode();

            await using var stream = await res.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            var types = type.Equals("all", StringComparison.OrdinalIgnoreCase)
                ? new[] { "selected", "events", "births", "deaths", "holidays" }
                : new[] { type };

            var list = new List<OnThisDayItemDto>();

            foreach (var t in types)
            {
                if (!doc.RootElement.TryGetProperty(t, out var arr) || arr.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var el in arr.EnumerateArray())
                {
                    var year = el.TryGetProperty("year", out var y) && y.TryGetInt32(out var yi) ? yi : 0;
                    var text = el.TryGetProperty("text", out var tx) ? (tx.GetString() ?? "") : "";

                    string? title = null, pageUrl = null, thumb = null;

                    if (el.TryGetProperty("pages", out var pages) && pages.ValueKind == JsonValueKind.Array)
                    {
                        var p0 = pages.EnumerateArray().FirstOrDefault();
                        if (p0.ValueKind != JsonValueKind.Undefined)
                        {
                            title = p0.TryGetProperty("displaytitle", out var dis) ? dis.GetString()
                                  : p0.TryGetProperty("title", out var ti) ? ti.GetString()
                                  : null;

                            if (p0.TryGetProperty("content_urls", out var cu)
                                && cu.TryGetProperty("desktop", out var desk)
                                && desk.TryGetProperty("page", out var page))
                                pageUrl = page.GetString();

                            if (p0.TryGetProperty("thumbnail", out var th)
                                && th.TryGetProperty("source", out var src))
                                thumb = src.GetString();
                        }
                    }

                    // حتی اگر title/pageUrl نباشد، خود متن رویداد را داریم
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        list.Add(new OnThisDayItemDto(
                            Type: t,
                            Year: year,
                            Text: text.Trim(),
                            Title: title,
                            PageUrl: pageUrl,
                            Thumbnail: thumb,
                            Language: language,
                            Month: month,
                            Day: day));
                    }
                }
            }

            return list.OrderByDescending(i => i.Year).ToList();
        }
    }
}
