using Microsoft.Extensions.Options;
using static ShikicinemaStatics.Posters.PosterListProviders.PosterListProviderStrategy;

namespace ShikicinemaStatics.Posters.PosterListProviders;

public class PosterListProviderFactory
{
    private readonly IOptionsMonitor<PostersLoaderOptions> _optionsMonitor;

    public PosterListProviderFactory(IOptionsMonitor<PostersLoaderOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public IPosterListProvider CreateProvider(IServiceProvider serviceProvider)
    {
        return _optionsMonitor.CurrentValue.ListProviderStrategy switch
        {
            StartToEndJpegAndWebp => serviceProvider.GetRequiredService<StartToEndJpegAndWebpPosterListProvider>(),
            EndToLoadedJpegAndWebp => serviceProvider.GetRequiredService<EndToLoadedJpegAndWebpPosterListProvider>(),
            _ => throw new NotSupportedException($"Unsupported strategy type: {_optionsMonitor.CurrentValue.ListProviderStrategy}")
        };
    }
}
