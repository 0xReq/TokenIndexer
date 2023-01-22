using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TokenIndexer.Domain.Initialization;

public static class AppInsightsInitExtension
{
    public static void AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
    {
        string? appInsightsInstrumentationKey = configuration["APPINSIGHTS_INSTRUMENTATIONKEY"] ?? null;
        
        services.AddApplicationInsightsTelemetry(appInsightsInstrumentationKey);
    }
}