using System.Net;
using System.Net.Http.Headers;
using Chh.Api.Tests.Common;
using Chh.Application.Contracts;
using Chh.Domain.Constants;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Chh.Api.Tests.Controllers;

/// <summary>
/// Guards <c>AdminController</c>'s route against `contracts/chh-api.v1.yaml`'s documented path,
/// and that the <c>[Authorize(Roles = RoleConstants.SystemAdmin)]</c> gate actually rejects both
/// an unauthenticated caller and one authenticated with a non-admin role (CHH-73).
/// </summary>
[Collection(ApiTestCollection.Name)]
public class AdminControllerRouteTests
{
    private readonly ApiWebApplicationFactory _factory;

    /// <summary>Creates the test class around the shared in-memory API host.</summary>
    /// <param name="factory">The shared API host fixture (see <see cref="ApiTestCollection"/>).</param>
    public AdminControllerRouteTests(ApiWebApplicationFactory factory) => _factory = factory;

    private string IssueToken(string role)
    {
        using var scope = _factory.Services.CreateScope();
        var jwtTokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        return jwtTokenGenerator.GenerateToken("9876543210", role).AccessToken;
    }

    [Fact]
    public async Task GetPendingFacilities_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/facilities/pending");

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must still resolve AdminController to contracts/chh-api.v1.yaml's documented path");
    }

    [Fact]
    public async Task GetPendingFacilities_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/facilities/pending");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPendingFacilities_WithNonAdminRole_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", IssueToken(RoleConstants.Individual));

        var response = await client.GetAsync("/api/v1/admin/facilities/pending");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReviewFacility_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsync($"/api/v1/admin/facilities/{Guid.NewGuid()}/verification", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must resolve AdminController's review action to contracts/chh-api.v1.yaml's documented path");
    }

    [Fact]
    public async Task ReviewFacility_WithNonAdminRole_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", IssueToken(RoleConstants.Individual));

        var response = await client.PatchAsync($"/api/v1/admin/facilities/{Guid.NewGuid()}/verification", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
