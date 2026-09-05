namespace Chh.Domain.Enums;

/// <summary>Blood group (PRD §8 Data Dictionary). Serializes over the wire as "A+"/"A-"/etc. via <c>Chh.Api.Json.BloodGroupJsonConverter</c>.</summary>
public enum BloodGroup
{
    /// <summary>A+</summary>
    APositive,

    /// <summary>A-</summary>
    ANegative,

    /// <summary>B+</summary>
    BPositive,

    /// <summary>B-</summary>
    BNegative,

    /// <summary>O+</summary>
    OPositive,

    /// <summary>O-</summary>
    ONegative,

    /// <summary>AB+</summary>
    ABPositive,

    /// <summary>AB-</summary>
    ABNegative
}
