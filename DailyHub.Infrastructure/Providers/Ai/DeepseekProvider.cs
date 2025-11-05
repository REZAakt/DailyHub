using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Runtime.CompilerServices;
using DailyHub.Shared.Abstractions.Chat;
using DailyHub.Shared.Dto.Ai;
using Microsoft.Extensions.Logging;

namespace DailyHub.Infrastructure.Providers.Chat;

public sealed class DeepseekProvider(HttpClient http, ILogger<DeepseekProvider> logger) : IDeepseekProvider
{
    static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public async IAsyncEnumerable<ChatChunkDto> StreamAsync(
        IReadOnlyList<ChatMessageDto> messages,
        ChatSettingsDto settings,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        req.Headers.Accept.ParseAdd("text/event-stream");

        var payload = new
        {
            model = settings.Model,
            messages = messages.Select(m => new { role = m.Role.ToString(), content = m.Content }).ToArray(),
            max_tokens = settings.MaxTokens,
            temperature = settings.Temperature,
            stream = true
        };
        req.Content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");

        using var res = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);

        // به‌جای throw و catch: همین‌جا هندل کن
        if (!res.IsSuccessStatusCode)
        {
            var reason = $"{(int)res.StatusCode} {res.ReasonPhrase}";
            yield return new ChatChunkDto(null, true, $"HTTP Error: {reason}");
            yield break;
        }

        await using var stream = await res.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var raw = await reader.ReadLineAsync();
            if (raw is null) break;
            if (raw.Length == 0) continue;
            if (!raw.StartsWith("data:")) continue;

            var data = raw["data:".Length..].Trim();
            if (data == "[DONE]")
            {
                yield return new ChatChunkDto(null, true, null);
                yield break;
            }

            // --- جمع کردن نتایج بدون yield داخل try/catch ---
            string? deltaOut = null;
            bool finishedNow = false;
            bool parseError = false;
            string? parseErrorMsg = null;

            try
            {
                using var doc = JsonDocument.Parse(data);
                var root = doc.RootElement;

                if (root.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var ch = choices[0];

                    if (ch.TryGetProperty("delta", out var deltaObj) &&
                        deltaObj.TryGetProperty("content", out var contentEl))
                    {
                        var d = contentEl.GetString();
                        if (!string.IsNullOrEmpty(d))
                            deltaOut = d; // فقط ذخیره، نه yield
                    }

                    if (ch.TryGetProperty("finish_reason", out var fr) &&
                        fr.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrEmpty(fr.GetString()))
                    {
                        finishedNow = true; // فقط فلگ کن
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "DeepSeek stream parse error: {Data}", data);
                parseError = true;
                parseErrorMsg = "Parse error";
            }

            // --- حالا بیرونِ try/catch yield کن ---
            if (parseError)
            {
                yield return new ChatChunkDto(null, true, parseErrorMsg);
                yield break;
            }

            if (deltaOut is not null)
                yield return new ChatChunkDto(deltaOut, false, null);

            if (finishedNow)
            {
                yield return new ChatChunkDto(null, true, null);
                yield break;
            }
        }

        // حلقه تمام شد؛ یک پایان ایمن بفرست
        yield return new ChatChunkDto(null, true, null);

    }

    public async Task<string> CompleteAsync(
        IReadOnlyList<ChatMessageDto> messages,
        ChatSettingsDto settings,
        CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        var payload = new
        {
            model = settings.Model,
            messages = messages.Select(m => new { role = m.Role.ToString(), content = m.Content }).ToArray(),
            max_tokens = settings.MaxTokens,
            temperature = settings.Temperature,
            stream = false
        };
        req.Content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");

        using var res = await http.SendAsync(req, ct);
        res.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        var root = doc.RootElement;

        var content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        return content ?? "";
    }
}
