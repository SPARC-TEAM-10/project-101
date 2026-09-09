using System.Net;
using Chh.Api.Tests.Common;
using FluentAssertions;
using Xunit;

namespace Chh.Api.Tests.Controllers;

/// <summary>
/// Guards the global "api/v1/[controller]" route convention for <c>EventsController</c> and its
/// role gate (CHH-38/US-CHH-005-01).
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
}
