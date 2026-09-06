namespace Chh.Domain.Enums;

/// <summary>
/// Gender (CHH-F02 Data Dictionary — "must select from list", exact options not enumerated by
/// the PRD). Assumption pending product confirmation: Male / Female / Other. Values are pinned
/// explicitly (not left to declaration order) — see <see cref="BloodGroup"/>'s doc comment for why.
/// </summary>
public enum Gender
{
    /// <summary>Male.</summary>
    Male = 1,

    /// <summary>Female.</summary>
    Female = 2,

    /// <summary>Other.</summary>
    Other = 3
}
