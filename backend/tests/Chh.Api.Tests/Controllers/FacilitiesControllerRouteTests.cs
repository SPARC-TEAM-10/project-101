using System.Net;
using Chh.Api.Tests.Common;
using FluentAssertions;
using Xunit;

namespace Chh.Api.Tests.Controllers;

/// <summary>
/// Guards the global "api/v1/[controller]" route convention for a controller whose action has no
/// template of its own (<c>[HttpPost("")]</c>, not bare <c>[HttpPost]</c>) — see the comment on
/// <c>FacilitiesController.RegisterAsync</c> for why the empty-string template is required. This
/// is also the regression guard for the reported 404: before this ticket, no controller resolved
/// this path at all.
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
            "the route convention must resolve FacilitiesController to contracts/chh-api.v1.yaml's documented path");
    }

    [Fact]
    public async Task PostFacilities_WithoutAuthorizationHeader_IsNotUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/facilities", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "facility registration is [AllowAnonymous], matching IndividualsController's precedent");
    }
}
