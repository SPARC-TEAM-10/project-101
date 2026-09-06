using System.Net;
using Chh.Api.Tests.Common;
using FluentAssertions;
using Xunit;

namespace Chh.Api.Tests.Controllers;

/// <summary>
/// Guards the global "api/v1/[controller]" route convention for a controller whose action has no
/// template of its own (<c>[HttpPost("")]</c>, not bare <c>[HttpPost]</c>) — see the comment on
/// <c>IndividualsController.RegisterAsync</c> for why the empty-string template is required.
/// </summary>
[Collection(ApiTestCollection.Name)]
public class IndividualsControllerRouteTests
{
    private readonly ApiWebApplicationFactory _factory;

    /// <summary>Creates the test class around the shared in-memory API host.</summary>
    /// <param name="factory">The shared API host fixture (see <see cref="ApiTestCollection"/>).</param>
    public IndividualsControllerRouteTests(ApiWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task PostIndividuals_UsesContractPath_IsRouted()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/v1/individuals", content: null);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            "the route convention must still resolve IndividualsController to contracts/chh-api.v1.yaml's documented path");
    }
}
