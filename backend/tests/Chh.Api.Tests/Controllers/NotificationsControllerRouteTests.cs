using System.Net;
using Chh.Api.Tests.Common;
using FluentAssertions;
using Xunit;

namespace Chh.Api.Tests.Controllers;

/// <summary>Guards the global "api/v1/[controller]" route convention for <c>NotificationsController</c> (CHH-34).</summary>
[Collection(ApiTestCollection.Name)]
public class NotificationsControllerRouteTests
{
    private readonly ApiWebApplicationFactory _factory;

    /// <summary>Creates the test class around the shared in-memory API host.</summary>
    /// <param name="factory">The shared API host fixture (see <see cref="ApiTestCollection"/>).</param>
    public NotificationsControllerRouteTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task GetMyNotifications_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/notifications/mine");

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must still resolve GetMyNotificationsAsync to contracts/chh-api.v1.yaml's documented path");
    }

    [Fact]
    public async Task GetMyNotifications_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/notifications/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PatchMarkRead_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsync($"/api/v1/notifications/{Guid.NewGuid()}/read", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must still resolve MarkReadAsync to contracts/chh-api.v1.yaml's documented path");
    }

    [Fact]
    public async Task PatchMarkRead_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsync($"/api/v1/notifications/{Guid.NewGuid()}/read", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PatchAccept_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsync($"/api/v1/notifications/{Guid.NewGuid()}/accept", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must still resolve AcceptAsync to contracts/chh-api.v1.yaml's documented path");
    }

    [Fact]
    public async Task PatchAccept_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsync($"/api/v1/notifications/{Guid.NewGuid()}/accept", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PatchDecline_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsync($"/api/v1/notifications/{Guid.NewGuid()}/decline", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must still resolve DeclineAsync to contracts/chh-api.v1.yaml's documented path");
    }

    [Fact]
    public async Task PatchDecline_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsync($"/api/v1/notifications/{Guid.NewGuid()}/decline", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
