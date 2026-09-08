using Chh.Domain.Enums;

namespace Chh.Application.Dtos;

/// <summary>
/// Internal transport shape joining a <c>DonorNotification</c> with its donor's <c>IndividualProfile</c>
/// identity (CHH-36) — never serialized directly over the wire (see <c>BloodRequestMatchStatusDto</c>'s
/// <c>DonorStatusDto</c> for the actual response shape, which redacts identity until Accepted).
/// </summary>
public record DonorNotificationWithDonorInfo
{
    /// <summary>The matched donor's <c>IndividualProfile</c> id.</summary>
    public required Guid DonorProfileId { get; init; }

    /// <summary>The donor's response so far (CHH-35).</summary>
    public required DonorResponseStatus ResponseStatus { get; init; }

    /// <summary>Read status (CHH-34 AC3) — feeds the requester dashboard's "Viewed" count.</summary>
    public required bool IsRead { get; init; }

    /// <summary>UTC timestamp the notification was created — the stable match order for anonymized labels.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>The donor's full name — only surfaced to the requester once <see cref="ResponseStatus"/> is Accepted.</summary>
    public required string DonorFullName { get; init; }

    /// <summary>The donor's mobile number — only surfaced to the requester once <see cref="ResponseStatus"/> is Accepted.</summary>
    public required string DonorMobileNumber { get; init; }
}
