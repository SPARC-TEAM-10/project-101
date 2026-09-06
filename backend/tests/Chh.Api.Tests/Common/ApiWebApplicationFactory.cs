using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

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

        // Jwt:SigningKeyBase64 is validated on start (ServiceCollectionExtensions.AddJwt) — these
        // route tests never issue a token, they just need the host to boot. Fixed, non-secret
        // 32-byte key; never used outside this in-memory test host.
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKeyBase64"] = "dGVzdC1vbmx5LXNpZ25pbmcta2V5LTMyLWJ5dGVzISE=",
            });
        });
    }
}
