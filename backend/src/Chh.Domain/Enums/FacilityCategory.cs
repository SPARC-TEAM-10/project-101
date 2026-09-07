namespace Chh.Domain.Enums;

/// <summary>Type of facility registering on the platform (CHH-78/US-CHH-003-01 AC1).</summary>
public enum FacilityCategory
{
    /// <summary>Hospital or blood bank — publishes blood requests and inventory.</summary>
    Hospital = 1,

    /// <summary>NGO — publishes community health events and camps.</summary>
    NGO = 2
}
