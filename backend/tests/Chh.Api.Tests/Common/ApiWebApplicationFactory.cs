using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Chh.Api.Tests.Common;

/// <summary>
/// Shared in-memory API host used by every integration test class in <c>Chh.Api.Tests</c>.
/// Customize environment/config overrides here in one place instead of per test class.
/// Runs under the "Testing" environment so <c>Program.cs</c> skips the startup EF Core
/// migration step — there is no real database available here.
/// </summary>
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }
}
