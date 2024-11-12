using Ardalis.GuardClauses;
using MemoryCache.Api;
using MemoryCache.Api.BackgroundServices;
using MemoryCache.Api.Constants;
using MemoryCache.Api.Settings;
using Microsoft.Extensions.Caching.Memory;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
        configuration.WriteTo.Console();
    });

var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var configurationSection = builder.Configuration.GetRequiredSection(nameof(CacheSettings));
var settings = configurationSection.Get<CacheSettings>();
Guard.Against.Null(settings, nameof(CacheSettings));
Guard.Against.Expression(
    func: x => x <= TimeSpan.Zero,
    input: settings.RefreshTimespan,
    message: "Wrong cache update interval",
    parameterName: nameof(settings.RefreshTimespan));
services.Configure<CacheSettings>(configurationSection);

services.AddMemoryCache();
services.AddHostedService<CacheBackgroundService>();

var application = builder.Build();

application.UseSerilogRequestLogging();

application.UseSwagger(options => { options.RouteTemplate = "openapi/{documentName}.json"; });
application.MapScalarApiReference(x => { x.Title = "MemoryCache API"; });
application.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

application.MapGet(
    "/get-time", (IMemoryCache memoryCache) =>
    {
        memoryCache.TryGetValue(CacheKeys.MyKey, out CacheModel? model);

        return model;
    });

application.Run();
