using System.Net;
using Chh.Api.Tests.Common;
using FluentAssertions;
using Xunit;

namespace Chh.Api.Tests.Controllers;

/// <summary>
/// Guards the global "api/v1/[controller]" route convention for <c>BloodRequestsController</c>,
/// and — since it's the first endpoint in this codebase to carry <c>[Authorize]</c> instead of
/// <c>[AllowAnonymous]</c> — that the JWT Bearer scheme actually rejects an unauthenticated
/// request rather than silently allowing it through.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class BloodRequestsControllerRouteTests
{
    private readonly ApiWebApplicationFactory _factory;

    /// <summary>Creates the test class around the shared in-memory API host.</summary>
    /// <param name="factory">The shared API host fixture (see <see cref="ApiTestCollection"/>).</param>
    public BloodRequestsControllerRouteTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task PostBloodRequests_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/blood-requests", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must still resolve BloodRequestsController to contracts/chh-api.v1.yaml's documented path");
    }

    [Fact]
    public async Task PostBloodRequests_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/blood-requests", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
