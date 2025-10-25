using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyHub.Infrastructure.DI;

using Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddDailyHubInfrastructure(this IServiceCollection services)
    {
        // اینجا بعداً HttpClientها و Providerها رجیستر می‌شن
        return services;
    }
}
