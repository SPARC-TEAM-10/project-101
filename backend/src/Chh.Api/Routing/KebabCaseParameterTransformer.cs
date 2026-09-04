using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace Chh.Api.Routing;

/// <summary>
/// Converts the <c>[controller]</c>/<c>[action]</c> route tokens to kebab-case (e.g.
/// <c>BloodRequests</c> to <c>blood-requests</c>), so <see cref="RoutePrefixConvention"/>'s
/// <c>api/v1/[controller]</c> route satisfies the hyphenated-URI rule in api-standards.md §1
/// without every controller writing out its own literal route string.
/// </summary>
public class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    /// <inheritdoc />
    public string? TransformOutbound(object? value) =>
        value is null ? null : Regex.Replace(value.ToString()!, "([a-z0-9])([A-Z])", "$1-$2").ToLowerInvariant();
}
