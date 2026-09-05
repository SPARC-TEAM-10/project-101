namespace Chh.Domain.Enums;

/// <summary>
/// Gender (CHH-F02 Data Dictionary — "must select from list", exact options not enumerated by
/// the PRD). Assumption pending product confirmation: Male / Female / Other.
/// </summary>
public enum Gender
{
    /// <summary>Male.</summary>
    Male,

    /// <summary>Female.</summary>
    Female,

    /// <summary>Other.</summary>
    Other
}
