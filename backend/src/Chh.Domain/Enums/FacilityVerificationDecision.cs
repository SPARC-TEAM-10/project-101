namespace Chh.Domain.Enums;

/// <summary>System Admin's decision when reviewing a pending facility (CHH-75/US-CHH-001-03).</summary>
public enum FacilityVerificationDecision
{
    /// <summary>Approve — facility moves to <see cref="FacilityVerificationStatus.Verified"/> (AC1).</summary>
    Approve = 1,

    /// <summary>Reject, with a mandatory reason — facility moves to <see cref="FacilityVerificationStatus.Rejected"/> (AC2).</summary>
    Reject = 2
}
