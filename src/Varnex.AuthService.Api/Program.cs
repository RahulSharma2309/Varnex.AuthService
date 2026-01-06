// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Varnex Enterprise">
//   © Varnex Enterprise. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Varnex.AuthService.Api;
using Varnex.AuthService.Core.Data;
using Ep.Platform.Hosting;
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Service", "AuthService")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/auth-service-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Auth Service");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Use Startup class for configuration
    var startup = new Startup(builder.Configuration);
    startup.ConfigureServices(builder.Services);

    var app = builder.Build();

    // Ensure database is created (for development)
    Log.Information("Ensuring database is created");
    await app.EnsureDatabaseAsync<AppDbContext>(applyMigrations: false);
    Log.Information("Database initialization completed");

    // Configure middleware pipeline
    startup.Configure(app, app.Environment);

    Log.Information("Auth Service started successfully on {Urls}", string.Join(", ", app.Urls));
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Auth Service terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}




