using Microsoft.Extensions.Configuration;
using Serilog;
using ShikicinemaStatics.Posters;

namespace ShikicinemaStatics;

internal static class Program
{
    public static async Task Main(string[] _)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

        try
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSerilog();
            serviceCollection.AddShikicinemaStatics(configuration);
            var serviceProvider = new DefaultServiceProviderFactory().CreateServiceProvider(serviceCollection);

            await using var scope = serviceProvider.CreateAsyncScope();
            var posterLoader = scope.ServiceProvider.GetRequiredService<IPostersLoader>();
            await posterLoader.LoadPostersAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Application failed");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
