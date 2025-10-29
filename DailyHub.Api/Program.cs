using DailyHub.Api.Endpoints;
using DailyHub.Api.Hubs;
using DailyHub.Infrastructure.DI;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true; // جزئیات خطا برای کلاینت
})
.AddJsonProtocol(o =>
{
    o.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.PayloadSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict;

    // اگر از DateOnly/TimeOnly در DTO استفاده می‌کنی و .NET نسخه‌ات پشتیبانی پیش‌فرض ندارد:
    // o.PayloadSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    // o.PayloadSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
});

builder.Services.AddDailyHubInfrastructure();

var app = builder.Build();

// --- Middleware ---
app.UseHttpsRedirection();
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

// --- Dev only ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// --- Routing ---
app.UseAuthorization();
app.MapControllers();
app.MapWeatherEndpoints();
app.MapHub<WeatherHub>("/hubs/weather");  // باید بعد از UseCors و قبل از Run بیاد

app.Run();
