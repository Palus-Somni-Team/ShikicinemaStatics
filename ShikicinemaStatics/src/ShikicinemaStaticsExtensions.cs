using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using ShikicinemaStatics.Posters;
using ShikicinemaStatics.Posters.Shikimori;

namespace ShikicinemaStatics;

public static class ShikicinemaStaticsExtensions
{
    public static WebApplicationBuilder AddShikicinemaStatics(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOptions<StaticFileOptions>()
            .Bind(builder.Configuration.GetSection(StaticFileOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return builder;
    }

    public static WebApplication UseShikicinemaStatics(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<StaticFileOptions>>();
        var isPhysicalPathAbsolute = Path.IsPathRooted(options.Value.PhysicalPath);
        var physicalPath = isPhysicalPathAbsolute
            ? options.Value.PhysicalPath
            : Path.Combine(Directory.GetCurrentDirectory(), options.Value.PhysicalPath);

        app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
        {
            RequestPath = options.Value.RequestPath,
            FileProvider = new PhysicalFileProvider(physicalPath),
        });

        return app;
    }

    public static WebApplicationBuilder AddPostersLoader(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<ShikimoriOptions>()
            .Bind(builder.Configuration.GetSection(ShikimoriOptions.SectionName))
            .ValidateDataAnnotations();

        builder.Services.AddOptions<PostersLoaderOptions>()
            .Bind(builder.Configuration.GetSection(PostersLoaderOptions.SectionName))
            .ValidateDataAnnotations();

        builder.Services.AddScoped<IPosterListProvider, PosterListProvider>();
        builder.Services.AddScoped<IPosterStore, PosterStore>();

        builder.Services.AddHostedService<PostersLoader>();

        builder.Services.AddHttpClient(nameof(PostersLoader), client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", nameof(ShikicinemaStatics));
        });
        builder.Services.AddHttpClient(nameof(PosterListProvider), (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptionsMonitor<ShikimoriOptions>>();
            client.BaseAddress = new Uri(options.CurrentValue.Host);
            client.DefaultRequestHeaders.Add("User-Agent", nameof(ShikicinemaStatics));
        });

        return builder;
    }
}
