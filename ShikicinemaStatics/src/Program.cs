using ShikicinemaStatics;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.AddShikicinemaStatics()
    .AddPostersLoader();

var app = builder.Build();

app.UseShikicinemaStatics();

app.Run();
