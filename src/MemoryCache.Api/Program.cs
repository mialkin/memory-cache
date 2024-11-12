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

var application = builder.Build();

application.UseSerilogRequestLogging();

application.UseSwagger(options => { options.RouteTemplate = "openapi/{documentName}.json"; });
application.MapScalarApiReference(x => { x.Title = "MemoryCache API"; });
application.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

application.MapGet("/say-hello", () => "Hello world!");

application.Run();
