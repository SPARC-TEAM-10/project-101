using System.Net;
using Chh.Api.Tests.Common;
using FluentAssertions;
using Xunit;

namespace Chh.Api.Tests.Controllers;

/// <summary>
/// Guards the global "api/v1/[controller]" route convention for <c>FacilitiesController</c>, and
/// that the JWT Bearer scheme rejects an unauthenticated request rather than silently allowing it
/// through — same shape as <see cref="BloodRequestsControllerRouteTests"/>.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class FacilitiesControllerRouteTests
{
    private readonly ApiWebApplicationFactory _factory;

    /// <summary>Creates the test class around the shared in-memory API host.</summary>
    /// <param name="factory">The shared API host fixture (see <see cref="ApiTestCollection"/>).</param>
    public FacilitiesControllerRouteTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task PostFacilities_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/facilities", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must still resolve FacilitiesController to contracts/chh-api.v1.yaml's documented path");
    }

    [Fact]
    public async Task PostFacilities_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/facilities", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
