using Polly;
using Polly.DependencyInjection;
using Polly.Retry;
using Polly.Telemetry;

namespace ShikicinemaStatics.Polly;

public static class PollyExtensions
{
    public const string ShikiClientPollyName = "ShikiClientPolly";

    public static void AddShikiResiliencePipeline(this IServiceCollection services)
    {
        services.AddResiliencePipeline(ShikiClientPollyName, BuildForShikiClient);
    }

    private static void BuildForShikiClient<TKey>(ResiliencePipelineBuilder builder,
        AddResiliencePipelineContext<TKey> context) where TKey : notnull
    {
        builder
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromMinutes(20),
                MaxRetryAttempts = int.MaxValue,
                BackoffType = DelayBackoffType.Exponential,
            })
            .ConfigureTelemetry(GetTelemetryOptions(context));
    }

    private static TelemetryOptions GetTelemetryOptions<TKey>(AddResiliencePipelineContext<TKey> context) where TKey : notnull
    {
        return new TelemetryOptions
        {
            LoggerFactory = context.ServiceProvider.GetRequiredService<ILoggerFactory>(),
            ResultFormatter = static (_, _) => string.Empty, // remove serialized result from logs
            SeverityProvider = args => args.Event.EventName switch
            {
                "OnRetry" => ResilienceEventSeverity.Debug, // reduce logs amount
                _ => args.Event.Severity
            },
        };
    }
}
