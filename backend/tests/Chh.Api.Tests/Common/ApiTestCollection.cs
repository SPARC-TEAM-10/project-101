using Xunit;

namespace Chh.Api.Tests.Common;

/// <summary>
/// xUnit collection sharing one <see cref="ApiWebApplicationFactory"/> instance across every test
/// class that opts in with <c>[Collection(ApiTestCollection.Name)]</c>, instead of each class
/// booting its own host via <c>IClassFixture</c>.
/// </summary>
[CollectionDefinition(Name)]
public class ApiTestCollection : ICollectionFixture<ApiWebApplicationFactory>
{
    /// <summary>Name passed to <c>[Collection(...)]</c> on test classes that share the fixture.</summary>
    public const string Name = "Api";
}
