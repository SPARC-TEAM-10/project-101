namespace Chh.Domain.Enums;

/// <summary>
/// Facility category (CHH-F03/CHH-F07). Matches the frontend's <c>FACILITY_CATEGORIES</c> and
/// <see cref="Chh.Domain.Constants.RoleConstants.Ngo"/> — wire value "Ngo", not "NGO".
/// </summary>
public enum FacilityCategory
{
    /// <summary>Hospital or blood bank — publishes blood requests and inventory.</summary>
    Hospital = 1,

    /// <summary>Non-governmental organization — publishes community health events and camps.</summary>
    Ngo = 2
}
