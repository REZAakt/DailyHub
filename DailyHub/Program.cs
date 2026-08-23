using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DailyHub;
using Radzen;
using DailyHub.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddLocalization();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<OnThisDayRealtimeService>();
builder.Services.AddScoped<NewsRealtimeService>();
builder.Services.AddScoped<WeatherRealtimeService>();
builder.Services.AddSingleton<CryptoRealtimeService>();
builder.Services.AddScoped<DeepseekRealtimeService>();
builder.Services.AddScoped<MetalsRealtimeService>();
builder.Services.AddScoped<RatesRealtimeService>();
builder.Services.AddScoped<CalendarRealtimeService>();

await builder.Build().RunAsync();
