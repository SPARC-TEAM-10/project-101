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

    [Fact]
    public async Task GetFacilitiesMe_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/facilities/me");

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must resolve FacilitiesController.GetMyFacilityAsync to contracts/chh-api.v1.yaml's documented /facilities/me path");
    }

    [Fact]
    public async Task GetFacilitiesMe_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/facilities/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "CHH-28's status dashboard is restricted to the Hospital/Ngo roles, unlike facility registration");
    }

    [Fact]
    public async Task PostUpload_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/v1/facilities/{Guid.NewGuid()}/upload", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must resolve UploadLicenseDocumentAsync to /facilities/{id}/upload — this is the regression guard for the reported 404");
    }

    [Fact]
    public async Task PostUpload_WithoutAuthorizationHeader_IsNotUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/v1/facilities/{Guid.NewGuid()}/upload", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "the upload happens right after anonymous registration, matching PostFacilities' precedent");
    }

    [Fact]
    public async Task GetFacilitiesSearch_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/facilities/search");

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must resolve SearchAsync to contracts/chh-api.v1.yaml's documented /facilities/search path");
    }

    [Fact]
    public async Task GetFacilitiesSearch_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/facilities/search");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "the Emergency Services Hub search requires a valid JWT (any authenticated role, incl. Guest) — api-standards.md §5");
    }

    [Fact]
    public async Task GetFacilitiesById_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/facilities/{Guid.NewGuid()}");

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must resolve GetPublicDetailAsync to contracts/chh-api.v1.yaml's documented /facilities/{id} path");
    }

    [Fact]
    public async Task GetFacilitiesById_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/facilities/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "the facility detail view requires a valid JWT (any authenticated role, incl. Guest) — api-standards.md §5");
    }
}
