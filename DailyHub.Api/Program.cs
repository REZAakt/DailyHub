using DailyHub.Api.Endpoints;
using DailyHub.Api.Hubs;
using DailyHub.Infrastructure.Caching;
using DailyHub.Infrastructure.DI;
using DailyHub.Shared.Abstractions.Crypto;
using DailyHub.Shared.Abstractions.News;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
//builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
//    .AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddMemoryCache();

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

builder.Services.AddHttpClient<ICryptoProvider, CoinGeckoProvider>("ICryptoProvider", (sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var apiKey = cfg["CoinGecko:CG-dpT4B1Mm77faTt8GrgWtynGC"];

    http.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
    http.DefaultRequestHeaders.UserAgent.ParseAdd("DailyHub/1.0 (+https://example.local)");
    http.DefaultRequestHeaders.Accept.ParseAdd("application/json");

    if (!string.IsNullOrWhiteSpace(apiKey))
        http.DefaultRequestHeaders.Add("x-cg-demo-api-key", apiKey); // یا x-cg-pro-api-key
})
.SetHandlerLifetime(TimeSpan.FromMinutes(10));

builder.Services.AddHttpClient("rss");
builder.Services.AddScoped<INewsProvider, RssNewsProvider>();
builder.Services.AddScoped<INewsCache, NewsCache>();
builder.Services.AddHttpClient<ICryptoProvider, CoinGeckoProvider>();
builder.Services.AddOtdProvider();
builder.Services.AddDailyHubInfrastructure();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
// --- Middleware ---
app.UseHttpsRedirection();
app.UseCors();
//app.UseCors("AllowAll");

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
app.MapHub<OnThisDayHub>("/hubs/otd");
app.MapHub<WeatherHub>("/hubs/weather");  // باید بعد از UseCors و قبل از Run بیاد
app.MapHub<NewsHub>("/hubs/news");
app.MapHub<CryptoHub>("/hubs/crypto");
app.Run();
