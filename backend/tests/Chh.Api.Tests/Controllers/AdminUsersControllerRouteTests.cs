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
/// Guards <c>AdminUsersController</c>'s route (its absolute <see cref="Microsoft.AspNetCore.Mvc.RouteAttribute"/>
/// override — see that class's doc comment for why) and its <c>[Authorize(Roles = SystemAdmin)]</c>
/// gate (CHH-76).
/// </summary>
[Collection(ApiTestCollection.Name)]
public class AdminUsersControllerRouteTests
{
    private readonly ApiWebApplicationFactory _factory;

    /// <summary>Creates the test class around the shared in-memory API host.</summary>
    /// <param name="factory">The shared API host fixture (see <see cref="ApiTestCollection"/>).</param>
    public AdminUsersControllerRouteTests(ApiWebApplicationFactory factory) => _factory = factory;

    private string IssueToken(string role)
    {
        using var scope = _factory.Services.CreateScope();
        var jwtTokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        return jwtTokenGenerator.GenerateToken("9876543210", role).AccessToken;
    }

    [Fact]
    public async Task SearchUsers_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/users");

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the absolute [Route] override must resolve AdminUsersController to api/v1/admin/users");
    }

    [Fact]
    public async Task SearchUsers_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchUsers_WithNonAdminRole_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", IssueToken(RoleConstants.Individual));

        var response = await client.GetAsync("/api/v1/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuspendUser_WithNonAdminRole_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", IssueToken(RoleConstants.Individual));

        var response = await client.PatchAsync($"/api/v1/admin/users/{Guid.NewGuid()}/suspend", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
