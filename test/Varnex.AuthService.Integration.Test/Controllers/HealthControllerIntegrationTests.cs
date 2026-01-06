using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Varnex.AuthService.Integration.Test.Controllers;

[Collection("VarnexAuthServiceIntegration")]
public class HealthControllerIntegrationTests
{
    private readonly VarnexAuthServiceFixture _fixture;

    public HealthControllerIntegrationTests(VarnexAuthServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Get_ReturnsOkAndHealthyStatus()
    {
        var response = await _fixture.Client.GetAsync("/api/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
        content.Should().Contain("auth-service");
    }
}








