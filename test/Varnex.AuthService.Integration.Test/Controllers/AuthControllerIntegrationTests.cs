using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Varnex.AuthService.Abstractions.DTOs;
using FluentAssertions;
using Xunit;

namespace Varnex.AuthService.Integration.Test.Controllers;

[Collection("VarnexAuthServiceIntegration")]
public class AuthControllerIntegrationTests
{
    private readonly VarnexAuthServiceFixture _fixture;

    public AuthControllerIntegrationTests(VarnexAuthServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Register_ValidData_ReturnsCreated()
    {
        var email = $"it-{Guid.NewGuid():N}@example.com";
        var password = "Password123!";
        var phoneNumber = $"+1{Random.Shared.NextInt64(1000000000, 9999999999)}";

        var registerDto = new RegisterDto
        {
            FullName = "Integration Test User",
            Email = email,
            Password = password,
            ConfirmPassword = password,
            PhoneNumber = phoneNumber,
            Address = "123 Integration St",
        };

        var response = await _fixture.Client.PostAsJsonAsync("/api/auth/register", registerDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        content.Should().NotBeNull();
        content!.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var email = $"it-{Guid.NewGuid():N}@example.com";
        var password = "Password123!";
        var phoneNumber = $"+1{Random.Shared.NextInt64(1000000000, 9999999999)}";
        await _fixture.RegisterAndLoginUser(email, password, "Login Test User", phoneNumber, "456 Login Ave");

        var loginDto = new LoginDto { Email = email, Password = password };
        var response = await _fixture.Client.PostAsJsonAsync("/api/auth/login", loginDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        authResponse.Should().NotBeNull();
        authResponse!.Token.Should().NotBeEmpty();
    }
}








