using Chh.Domain.Enums;

namespace Chh.Domain.Constants;

/// <summary>
/// Clinical notation ("A+", "AB-", etc.) for <see cref="BloodGroup"/> — used wherever a
/// human-readable blood group is needed outside JSON serialization (e.g. CHH-34's SMS copy).
/// Mirrors <c>Chh.Api.Json.BloodGroupJsonConverter</c>'s wire mapping; kept here instead of in
/// <c>Chh.Api</c> so <c>Chh.Application</c> can use it without an upward dependency.
/// </summary>
public static class BloodGroupDisplay
{
    private static readonly IReadOnlyDictionary<BloodGroup, string> Notation = new Dictionary<BloodGroup, string>
    {
        [BloodGroup.APositive] = "A+",
        [BloodGroup.ANegative] = "A-",
        [BloodGroup.BPositive] = "B+",
        [BloodGroup.BNegative] = "B-",
        [BloodGroup.OPositive] = "O+",
        [BloodGroup.ONegative] = "O-",
        [BloodGroup.ABPositive] = "AB+",
        [BloodGroup.ABNegative] = "AB-"
    };

    /// <summary>Returns the clinical notation for <paramref name="bloodGroup"/> (e.g. "O+").</summary>
    public static string ToClinicalNotation(BloodGroup bloodGroup) => Notation[bloodGroup];
}
