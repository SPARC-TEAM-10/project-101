using System.Net;
using Chh.Api.Tests.Common;
using FluentAssertions;
using Xunit;

namespace Chh.Api.Tests.Controllers;

/// <summary>
/// Guards the global "api/v1/[controller]" route convention for <c>EventsController</c> and its
/// role gates (CHH-38/US-CHH-005-01, CHH-39/US-CHH-005-02).
/// </summary>
[Collection(ApiTestCollection.Name)]
public class EventsControllerRouteTests
{
    private readonly ApiWebApplicationFactory _factory;

    /// <summary>Creates the test class around the shared in-memory API host.</summary>
    /// <param name="factory">The shared API host fixture (see <see cref="ApiTestCollection"/>).</param>
    public EventsControllerRouteTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task PostEvents_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/events", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must resolve EventsController to contracts/chh-api.v1.yaml's documented path");
    }

    [Fact]
    public async Task PostEvents_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/events", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
            "event creation requires the Hospital/Ngo role, unlike facility registration");
    }

    [Fact]
    public async Task GetEventsSearch_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/events/search?latitude=9.93&longitude=76.26&radiusKm=25");

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must resolve EventsController.SearchAsync to contracts/chh-api.v1.yaml's documented /events/search path");
    }

    [Fact]
    public async Task GetEventsSearch_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/events/search?latitude=9.93&longitude=76.26&radiusKm=25");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEventById_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/events/{Guid.NewGuid()}");

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must resolve EventsController.GetByIdAsync to contracts/chh-api.v1.yaml's documented /events/{id} path — a missing event still routes, it just 404s inside the handler, but an unauthenticated request 401s first");
    }

    [Fact]
    public async Task GetEventById_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/events/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostEventRsvp_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/v1/events/{Guid.NewGuid()}/rsvp", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteEventRsvp_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/v1/events/{Guid.NewGuid()}/rsvp");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEventsMine_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/events/mine");

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must resolve EventsController.GetMineAsync to contracts/chh-api.v1.yaml's documented /events/mine path");
    }

    [Fact]
    public async Task GetEventsMine_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/events/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PatchEventById_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsync($"/api/v1/events/{Guid.NewGuid()}", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostEventCancel_UsesContractPath_IsRouted()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/v1/events/{Guid.NewGuid()}/cancel", content: null);

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must resolve EventsController.CancelAsync to contracts/chh-api.v1.yaml's documented /events/{id}/cancel path");
    }

    [Fact]
    public async Task PostEventCancel_WithoutAuthorizationHeader_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync($"/api/v1/events/{Guid.NewGuid()}/cancel", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
