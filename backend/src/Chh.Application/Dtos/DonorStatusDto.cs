namespace Chh.Application.Dtos;

/// <summary>
/// One donor's status as shown to the requester (CHH-36 AC2), shaped per the donor-list
/// visibility rule: identity is redacted behind <see cref="Label"/> ("Donor 1", "Donor 2", ...)
/// until the donor accepts — Accepting is what reveals a real name and number, in both the guest
/// and registered requester views. See <c>BloodRequestMatchService</c> for how <see cref="Label"/>
/// and <see cref="MobileNumber"/> are populated differently for a guest vs. a registered requester.
/// </summary>
public record DonorStatusDto
{
    /// <summary>
    /// "Donor 1"/"Donor 2"/... for a still-pending match (registered requester view only — a
    /// guest requester's list never contains a pending row at all), or the donor's real full name
    /// once <see cref="IsAccepted"/> is true.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>The donor's mobile number — populated only when <see cref="IsAccepted"/> is true.</summary>
    public string? MobileNumber { get; init; }

    /// <summary>Whether this donor has accepted (AC2's trigger for revealing identity).</summary>
    public required bool IsAccepted { get; init; }
}
