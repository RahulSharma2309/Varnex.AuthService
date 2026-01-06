using Varnex.AuthService.Core.Business;
using Varnex.AuthService.Core.Data;
using Varnex.AuthService.Core.Repository;
using Ep.Platform.DependencyInjection;
using Ep.Platform.Hosting;

namespace Varnex.AuthService.Api;

/// <summary>
/// Application startup configuration for the Auth service.
/// </summary>
public class Startup
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Configure services for dependency injection.
    /// </summary>
    /// <param name="services">The services collection used for dependency injection.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // Add controllers
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // Use Ep.Platform extensions
        services.AddEpSwaggerWithJwt("Auth Service", "v1");
        services.AddEpDefaultCors("AllowLocalhost3000", new[] { "http://localhost:3000" });

        // Database (Platform handles EF Core)
        services.AddEpSqlServerDbContext<AppDbContext>(Configuration);

        // HttpClient for User Service (Platform handles HttpClient + Polly)
        services.AddEpHttpClient("user", Configuration, "ServiceUrls:UserService");

        // JWT Authentication (Platform handles JWT Bearer configuration)
        services.AddEpJwtAuth(Configuration);

        // Security Services (Platform provides password hashing and JWT token generation)
        services.AddEpSecurityServices();

        // Register business and repository services (Auth Service specific)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, Core.Business.AuthService>();
    }

    /// <summary>
    /// Configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="env">The web hosting environment.</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.UseCors("AllowLocalhost3000");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}




