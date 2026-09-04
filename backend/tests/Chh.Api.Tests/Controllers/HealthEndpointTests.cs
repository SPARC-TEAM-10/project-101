using System.Net;
using Chh.Api.Tests.Common;
using FluentAssertions;
using Xunit;

namespace Chh.Api.Tests.Controllers;

/// <summary>Smoke tests proving the API host boots and serves the liveness probe.</summary>
[Collection(ApiTestCollection.Name)]
public class HealthEndpointTests
{
    private readonly ApiWebApplicationFactory _factory;

    /// <summary>Creates the test class around the shared in-memory API host.</summary>
    /// <param name="factory">The shared API host fixture (see <see cref="ApiTestCollection"/>).</param>
    public HealthEndpointTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetHealth_WhenHostIsRunning_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
