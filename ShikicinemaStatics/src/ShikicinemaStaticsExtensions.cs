using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ShikicinemaStatics.Posters;
using ShikicinemaStatics.Posters.PosterListProviders;
using ShikicinemaStatics.Posters.Shikimori;

namespace ShikicinemaStatics;

public static class ShikicinemaStaticsExtensions
{
    public static void AddShikicinemaStatics(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<StaticFileOptions>()
            .Bind(configuration.GetSection(StaticFileOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ShikimoriOptions>()
            .Bind(configuration.GetSection(ShikimoriOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddOptions<PostersLoaderOptions>()
            .Bind(configuration.GetSection(PostersLoaderOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddScoped<StartToEndPosterListProvider>();
        services.AddScoped<EndToLoadedPosterListProvider>();

        services.AddScoped<PosterListProviderFactory>();
        services.AddScoped<IPosterListProvider>(service =>
            service.GetRequiredService<PosterListProviderFactory>().CreateProvider(service)
        );

        services.AddScoped<IPosterStore, PosterStore>();
        services.AddScoped<IPostersLoader, PostersLoader>();

        services.AddHttpClient(nameof(PosterListProviderBase), (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<ShikimoriOptions>>();
            client.BaseAddress = new Uri(options.CurrentValue.Host);
            client.DefaultRequestHeaders.Add("User-Agent", nameof(ShikicinemaStatics));
        });
    }
}
