using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Varnex.AuthService.Api;
using Varnex.AuthService.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System.Net.Http.Json;
using Varnex.AuthService.Abstractions.DTOs;
using Moq;
using Moq.Protected;
using System.Threading;
using System.Net;
using Ep.Platform.Security;
using Varnex.AuthService.Integration.Test.Fakes;
using Microsoft.Extensions.Http;

namespace Varnex.AuthService.Integration.Test;

public class VarnexAuthServiceFixture : IAsyncLifetime
{
    private readonly WebApplicationFactory<Startup> _factory;
    public HttpClient Client { get; }

    public VarnexAuthServiceFixture()
    {
        _factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStrings:DefaultConnection", "Server=(localdb)\\MSSQLLocalDB;Database=AuthServiceTestDb;Trusted_Connection=True;MultipleActiveResultSets=true" },
                    { "ServiceUrls:UserService", "http://user-service-mock:3001" },
                    { "Jwt:Key", "your-super-secret-key-that-should-be-at-least-32-characters-long-for-security" },
                    { "Jwt:Issuer", "Varnex Enterprise" },
                    { "Jwt:Audience", "Varnex Enterprise-Users" }
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    // Use a fixed name so the same in-memory store is shared across requests within this test server.
                    options.UseInMemoryDatabase("InMemoryAuthTestDb");
                });

                // Override password hasher to keep passwords in plain text for deterministic login in tests.
                var existingHasher = services.FirstOrDefault(d => d.ServiceType == typeof(IPasswordHasher));
                if (existingHasher != null)
                {
                    services.Remove(existingHasher);
                }
                services.AddSingleton<IPasswordHasher, FakePasswordHasher>();

                // Mock HttpMessageHandler for "user" client
                var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
                
                // Setup a default response for everything to avoid 503s
                handlerMock
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>()
                    )
                    .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) => {
                        if (req.Method == HttpMethod.Get && req.RequestUri!.PathAndQuery.Contains("/phone-exists/"))
                        {
                            return new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.OK,
                                Content = JsonContent.Create(new { exists = false })
                            };
                        }
                        if (req.Method == HttpMethod.Post && req.RequestUri!.PathAndQuery.Contains("/api/users"))
                        {
                            return new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.Created,
                                Content = JsonContent.Create(new { id = Guid.NewGuid(), userId = Guid.NewGuid() })
                            };
                        }
                        return new HttpResponseMessage(HttpStatusCode.NotFound);
                    });

                services.AddHttpClient("user")
                    .ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object);

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                }
            });
        });

        Client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        var response = await Client.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }

    public async Task<string> RegisterAndLoginUser(string email, string password, string fullName, string phoneNumber, string address)
    {
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            Address = address
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        registerResponse.EnsureSuccessStatusCode();

        var loginDto = new LoginDto { Email = email, Password = password };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
        return authResponse!.Token;
    }
}








