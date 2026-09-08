using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>Request body for <c>PATCH /api/v1/admin/facilities/{id}/verification</c> (CHH-75/US-CHH-001-03).</summary>
public record UpdateFacilityVerificationRequest
{
    /// <summary>Approve or reject.</summary>
    public required FacilityVerificationDecision Decision { get; init; }

    /// <summary>Mandatory when <see cref="Decision"/> is <see cref="FacilityVerificationDecision.Reject"/> (AC2); ignored otherwise.</summary>
    public string? RejectionReason { get; init; }
}
