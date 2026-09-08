namespace Chh.Domain.Enums;

/// <summary>Facility sub-category — a finer classification within <see cref="FacilityCategory"/> (CHH-78 follow-up).</summary>
public enum FacilitySubCategory
{
    /// <summary>Government-run hospital.</summary>
    Government = 1,

    /// <summary>Privately-run hospital.</summary>
    Private = 2,

    /// <summary>Trust-run hospital, or an NGO registered as a trust.</summary>
    Trust = 3,

    /// <summary>NGO registered as a society.</summary>
    RegisteredSociety = 4,

    /// <summary>NGO incorporated as a Section 8 company.</summary>
    Section8Company = 5
}
