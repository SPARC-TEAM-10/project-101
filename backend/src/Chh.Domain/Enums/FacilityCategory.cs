namespace Chh.Domain.Enums;

/// <summary>
/// Facility category (CHH-F03/CHH-F07). Matches the frontend's <c>FACILITY_CATEGORIES</c> —
/// the wire value must be "NGO" (not "Ngo"), since the frontend (CHH-78, already shipped) hardcodes
/// that exact string.
/// </summary>
public enum FacilityCategory
{
    /// <summary>Hospital or blood bank — publishes blood requests and inventory.</summary>
    Hospital = 1,

    /// <summary>Non-governmental organization — publishes community health events and camps.</summary>
    NGO = 2
}
