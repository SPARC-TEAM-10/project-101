using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Chh.Api.Routing;

/// <summary>
/// Prepends a fixed route template (currently just <c>api/v1</c>) to every controller's route, so
/// the version prefix is declared once — in <c>Program.cs</c> — instead of on a <c>[Route]</c>
/// attribute repeated by each controller (api-standards.md §1 URL versioning).
/// </summary>
/// <remarks>
/// Deliberately does NOT also derive the resource segment from the controller's class name via a
/// <c>[controller]</c> token (an earlier version of this prefix was <c>api/v1/[controller]</c>).
/// That worked for resources whose kebab-cased class name happened to match the desired URL
/// (<c>IndividualsController</c> -&gt; "individuals"), but breaks for anything that doesn't —
/// <c>AdminFacilitiesController</c> needs the two-segment <c>admin/facilities</c>, not the
/// single kebab-cased segment <c>admin-facilities</c> the token would have produced, and
/// <c>[controller]</c> combines with a controller's *own* <c>[Route]</c> rather than being
/// replaced by it, so simply overriding won't help — the two would concatenate into nonsense.
/// Every controller now states its resource path explicitly via its own <c>[Route(...)]</c>
/// (e.g. <c>[Route("auth/otp")]</c>, <c>[Route("admin/facilities")]</c>), combined with this
/// convention's "api/v1" the normal way <c>[Route]</c> attributes combine with an outer prefix.
/// <see cref="KebabCaseParameterTransformer"/> still applies to whatever tokens a controller's own
/// route does use, if any.
/// </remarks>
public class RoutePrefixConvention : IControllerModelConvention
{
    private readonly AttributeRouteModel _prefix;

    /// <summary>Creates the convention with the given route template.</summary>
    /// <param name="prefix">The route template to prepend, e.g. <c>"api/v1/[controller]"</c>.</param>
    public RoutePrefixConvention(string prefix)
    {
        _prefix = new AttributeRouteModel(new RouteAttribute(prefix));
    }

    /// <inheritdoc />
    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
        {
            selector.AttributeRouteModel = selector.AttributeRouteModel is null
                ? _prefix
                : AttributeRouteModel.CombineAttributeRouteModel(_prefix, selector.AttributeRouteModel);
        }
    }
}
