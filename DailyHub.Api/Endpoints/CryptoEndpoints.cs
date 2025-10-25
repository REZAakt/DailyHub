namespace DailyHub.Api.Endpoints;

using DailyHub.Shared.Abstractions.Crypto;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

public static class CryptoEndpoints
{
    public static IEndpointRouteBuilder MapCryptoEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/crypto/prices", (string symbols, ICryptoProvider provider) =>
        {
            // Implement later
            throw new NotImplementedException();
        });

        return app;
    }
}
