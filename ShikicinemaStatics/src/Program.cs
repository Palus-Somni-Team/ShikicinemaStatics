using Serilog;
using ShikicinemaStatics;
using ShikicinemaStatics.Polly;

var builder = WebApplication.CreateSlimBuilder(args);
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

try
{
    builder.Logging.ClearProviders();
    builder.Services.AddSerilog();
    builder.Services.AddShikiResiliencePipeline();

    builder.AddShikicinemaStatics()
        .AddPostersLoader();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseShikicinemaStatics();

    app.Run();
}
catch (Exception e)
{
    Log.Error(e, "Application failed");
}
finally
{
    Log.CloseAndFlush();
}
