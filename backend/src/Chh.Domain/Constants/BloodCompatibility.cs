using Chh.Domain.Enums;

namespace Chh.Domain.Constants;

/// <summary>
/// Standard donor-to-recipient blood compatibility matrix (US-CHH-004-02/CHH-80 AC1). O- is the
/// universal donor (can give to all 8 groups); AB+ is the universal recipient (can receive from
/// all 8 groups, but can only donate to AB+).
/// </summary>
public static class BloodCompatibility
{
    private static readonly Dictionary<BloodGroup, IReadOnlySet<BloodGroup>> CompatibleDonorsByRequestedGroup = new()
    {
        [BloodGroup.ONegative] = new HashSet<BloodGroup> { BloodGroup.ONegative },
        [BloodGroup.OPositive] = new HashSet<BloodGroup> { BloodGroup.ONegative, BloodGroup.OPositive },
        [BloodGroup.ANegative] = new HashSet<BloodGroup> { BloodGroup.ONegative, BloodGroup.ANegative },
        [BloodGroup.APositive] = new HashSet<BloodGroup> { BloodGroup.ONegative, BloodGroup.OPositive, BloodGroup.ANegative, BloodGroup.APositive },
        [BloodGroup.BNegative] = new HashSet<BloodGroup> { BloodGroup.ONegative, BloodGroup.BNegative },
        [BloodGroup.BPositive] = new HashSet<BloodGroup> { BloodGroup.ONegative, BloodGroup.OPositive, BloodGroup.BNegative, BloodGroup.BPositive },
        [BloodGroup.ABNegative] = new HashSet<BloodGroup> { BloodGroup.ONegative, BloodGroup.ANegative, BloodGroup.BNegative, BloodGroup.ABNegative },
        [BloodGroup.ABPositive] = new HashSet<BloodGroup>
        {
            BloodGroup.ONegative, BloodGroup.OPositive,
            BloodGroup.ANegative, BloodGroup.APositive,
            BloodGroup.BNegative, BloodGroup.BPositive,
            BloodGroup.ABNegative, BloodGroup.ABPositive
        }
    };

    /// <summary>Returns the set of donor blood groups compatible with the given requested (recipient) blood group.</summary>
    /// <param name="requestedGroup">The blood group on the request (the recipient's blood group).</param>
    public static IReadOnlySet<BloodGroup> GetCompatibleDonorGroups(BloodGroup requestedGroup) =>
        CompatibleDonorsByRequestedGroup[requestedGroup];
}
