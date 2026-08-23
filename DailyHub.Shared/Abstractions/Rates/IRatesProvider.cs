namespace DailyHub.Shared.Abstractions.Rates;

using DailyHub.Shared.Dto.Rates;

public interface IRatesProvider
{
    Task<IReadOnlyList<FxQuoteDto>> GetRatesAsync(string baseCurrency, IEnumerable<string> symbols, CancellationToken ct = default);
}
