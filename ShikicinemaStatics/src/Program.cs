using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<ShikicinemaStatics.StaticFileOptions>()
    .Bind(builder.Configuration.GetSection(ShikicinemaStatics.StaticFileOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var app = builder.Build();

var options = app.Services.GetRequiredService<IOptions<ShikicinemaStatics.StaticFileOptions>>();
var isPhysicalPathAbsolute = Path.IsPathRooted(options.Value.PhysicalPath);
var physicalPath = isPhysicalPathAbsolute
    ? options.Value.PhysicalPath
    : Path.Combine(Directory.GetCurrentDirectory(), options.Value.PhysicalPath);

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = options.Value.RequestPath,
    FileProvider = new PhysicalFileProvider(physicalPath),
});

app.Run();
