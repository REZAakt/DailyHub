using DailyHub.Infrastructure.Providers.Otd;
using DailyHub.Shared.Abstractions.OnThisDay;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.DI
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOtdProvider(this IServiceCollection services)
        {
            services.AddHttpClient<IOnThisDayProvider, WikimediaOnThisDayProvider>(c =>
            {
                c.BaseAddress = new Uri("https://api.wikimedia.org/");
                // User-Agent را هم Provider روی هر درخواست ست می‌کند
                c.Timeout = TimeSpan.FromSeconds(10);
            });
            return services;
        }
    }

}
