using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Chh.Api.Routing;

/// <summary>
/// Prepends a fixed route template (e.g. <c>api/v1/[controller]</c>) to every controller's route,
/// so the version prefix is declared once — in <c>Program.cs</c> — instead of on a <c>[Route]</c>
/// attribute repeated by each controller (api-standards.md §1 URL versioning). The <c>[controller]</c>
/// token is resolved per-controller same as it would be on a class-level <c>[Route]</c> attribute.
/// Combined with <see cref="KebabCaseParameterTransformer"/>, a controller named e.g.
/// <c>BloodRequestsController</c> resolves to <c>api/v1/blood-requests</c> with no route
/// attribute of its own.
/// </summary>
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
