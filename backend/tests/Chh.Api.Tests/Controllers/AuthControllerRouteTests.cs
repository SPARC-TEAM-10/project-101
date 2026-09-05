using System.Net;
using Chh.Api.Tests.Common;
using FluentAssertions;
using Xunit;

namespace Chh.Api.Tests.Controllers;

/// <summary>
/// Guards the global "api/v1/[controller]" route convention (<c>Chh.Api.Routing.RoutePrefixConvention</c>,
/// registered in <c>Program.cs</c>) — a controller with no <c>[Route]</c> attribute of its own must
/// still resolve under its documented contract path.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class AuthControllerRouteTests
{
    private readonly ApiWebApplicationFactory _factory;

    /// <summary>Creates the test class around the shared in-memory API host.</summary>
    /// <param name="factory">The shared API host fixture (see <see cref="ApiTestCollection"/>).</param>
    public AuthControllerRouteTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task PostOtpRequest_UsesContractPath_IsRouted()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/v1/auth/otp/request", content: null);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must still resolve AuthController to contracts/chh-api.v1.yaml's documented path");
    }
}
