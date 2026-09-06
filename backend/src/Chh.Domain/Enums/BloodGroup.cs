namespace Chh.Domain.Enums;

/// <summary>
/// Blood group (PRD §8 Data Dictionary). Serializes over the wire as "A+"/"A-"/etc. via
/// <c>Chh.Api.Json.BloodGroupJsonConverter</c>. Values are pinned explicitly (not left to
/// declaration order) since <see cref="Chh.Infrastructure.Persistence.Configurations.IndividualProfileConfiguration"/>
/// doesn't store the numeric value (it converts to the member name string), but validators and
/// any future numeric handling shouldn't silently shift if a member is inserted later.
/// </summary>
public enum BloodGroup
{
    /// <summary>A+</summary>
    APositive = 1,

    /// <summary>A-</summary>
    ANegative = 2,

    /// <summary>B+</summary>
    BPositive = 3,

    /// <summary>B-</summary>
    BNegative = 4,

    /// <summary>O+</summary>
    OPositive = 5,

    /// <summary>O-</summary>
    ONegative = 6,

    /// <summary>AB+</summary>
    ABPositive = 7,

    /// <summary>AB-</summary>
    ABNegative = 8
}
