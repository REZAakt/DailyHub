namespace DailyHub.Api.Endpoints;

using DailyHub.Shared.Abstractions.Crypto;

public static class CryptoEndpoints
{
    public static IEndpointRouteBuilder MapCryptoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/crypto/prices", async (string symbols, ICryptoProvider provider, CancellationToken ct) =>
        {
            try
            {
                var ids = symbols.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var items = await provider.GetAsync(ids, ct);
                return Results.Ok(items);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        return app;
    }
}
