namespace Chh.Application.Dtos;

/// <summary>Request body for <c>PATCH /api/v1/admin/users/{id}/suspend</c> (CHH-76/US-CHH-001-04 AC1).</summary>
public record SuspendUserRequest
{
    /// <summary>Mandatory reason for the suspension.</summary>
    public required string Reason { get; init; }
}
