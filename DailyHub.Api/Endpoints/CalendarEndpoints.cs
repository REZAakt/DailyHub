namespace DailyHub.Api.Endpoints;

using DailyHub.Shared.Abstractions.Calendar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

public static class CalendarEndpoints
{
    public static IEndpointRouteBuilder MapCalendarEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/calendar/holidays", (string country, int year, ICalendarProvider provider) =>
        {
            // Implement later
            throw new NotImplementedException();
        });

        return app;
    }
}
