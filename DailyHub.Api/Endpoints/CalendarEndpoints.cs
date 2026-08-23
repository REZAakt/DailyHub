namespace DailyHub.Api.Endpoints;

using DailyHub.Shared.Abstractions.Calendar;

public static class CalendarEndpoints
{
    public static IEndpointRouteBuilder MapCalendarEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/calendar/holidays", async (string country, int? year, ICalendarProvider provider, CancellationToken ct) =>
        {
            try
            {
                var y = year is null or 0 ? DateTime.UtcNow.Year : year.Value;
                var holidays = await provider.GetHolidaysAsync(country, y, ct);
                return Results.Ok(holidays);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });

        return app;
    }
}
