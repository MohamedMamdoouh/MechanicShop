using MechanicShop.Api;
using MechanicShop.Application.Features;
using MechanicShop.Infrastructure;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Realtime;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // We can use either Swagger or Scalar UI in development
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MechanicShop API V1");
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.EnableFilter();
        options.EnablePersistAuthorization();
    });

    app.MapScalarApiReference(options =>
    {
        options.Title = "MechanicShop API";
        options.Theme = ScalarTheme.DeepSpace;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseHsts();
}

app.UsePresentation(builder.Configuration);

app.MapControllers();
app.MapHub<WorkOrderHub>(WorkOrderHub.HubUrl);
app.MapHealthChecks("/health").AllowAnonymous();
app.MapPrometheusScrapingEndpoint()
    .RequireAuthorization();

await app.RunAsync();
