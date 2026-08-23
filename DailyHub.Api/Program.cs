using DailyHub.Api.Endpoints;
using DailyHub.Api.Hubs;
using DailyHub.Infrastructure.Caching;
using DailyHub.Infrastructure.DI;
using DailyHub.Infrastructure.Providers.Chat;
using DailyHub.Shared.Abstractions.Calendar;
using DailyHub.Shared.Abstractions.Chat;
using DailyHub.Shared.Abstractions.Crypto;
using DailyHub.Shared.Abstractions.Metals;
using DailyHub.Shared.Abstractions.News;
using DailyHub.Shared.Abstractions.Rates;
using DailyHub.Infrastructure.Providers.Calendar;
using DailyHub.Infrastructure.Providers.Metals;
using DailyHub.Infrastructure.Providers.Rates;
using System.Net.Http.Headers;

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

builder.Services.AddHttpClient<IDeepseekProvider, DeepseekProvider>(client =>
{
    client.BaseAddress = new Uri("https://api.deepseek.com/v1/"); // توجه: v1
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", builder.Configuration["DeepSeek:ApiKey"]); // از secrets/env بخوان
    client.DefaultRequestHeaders.UserAgent.ParseAdd("DailyHub/1.0 (+Rreza.Aak@gmail.com)");
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


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowGitHubPages",
//        policy =>
//        {
//            policy.WithOrigins(
//                "https://username.github.io",
//                "https://username.github.io/my-client-frontend"
//            )
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//        });
//});


// 2) استفاده از policy قبل از UseAuthorization

builder.Services.AddHttpClient("rss");
builder.Services.AddScoped<INewsProvider, RssNewsProvider>();
builder.Services.AddScoped<INewsCache, NewsCache>();
builder.Services.AddHttpClient<ICryptoProvider, CoinGeckoProvider>();
builder.Services.AddOtdProvider();
builder.Services.AddDailyHubInfrastructure();

// --- سرویس‌های جدید: طلا، ارز، تقویم ---
builder.Services.AddHttpClient<IMetalsProvider, MetalsLiveProvider>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(15);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("DailyHub/1.0");
});
builder.Services.AddHttpClient<IRatesProvider, FrankfurterRatesProvider>(c =>
{
    c.BaseAddress = new Uri("https://api.frankfurter.dev/v1/");
    c.Timeout = TimeSpan.FromSeconds(15);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("DailyHub/1.0");
});
builder.Services.AddHttpClient<ICalendarProvider, NagerDateProvider>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(15);
    c.DefaultRequestHeaders.UserAgent.ParseAdd("DailyHub/1.0");
});


var app = builder.Build();

//app.UseCors("AllowGitHubPages");
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
app.MapNewsEndpoints();
app.MapCryptoEndpoints();
app.MapCalendarEndpoints();
app.MapHub<OnThisDayHub>("/hubs/otd");
app.MapHub<WeatherHub>("/hubs/weather");  // باید بعد از UseCors و قبل از Run بیاد
app.MapHub<NewsHub>("/hubs/news");
app.MapHub<CryptoHub>("/hubs/crypto");
app.MapHub<DeepseekHub>("/hubs/deepseek");
app.MapHub<MetalsHub>("/hubs/metals");
app.MapHub<RatesHub>("/hubs/rates");
app.MapHub<CalendarHub>("/hubs/calendar");

app.Run();
