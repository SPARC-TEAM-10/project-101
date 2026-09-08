using Chh.Domain.Entities;

namespace Chh.Application.Factories;

/// <summary>
/// Constructs <see cref="DonorNotification"/> instances (CHH-34) — kept out of the entity,
/// matching <see cref="BloodRequestFactory"/>'s and <see cref="IndividualProfileFactory"/>'s pattern.
/// </summary>
public static class DonorNotificationFactory
{
    /// <summary>Creates a new, unread donor notification for a matched blood request.</summary>
    /// <param name="bloodRequest">The matched blood request — only its off-app-safe facts are copied (AC2).</param>
    /// <param name="donorProfileId">The matched donor's <c>IndividualProfile</c> id.</param>
    /// <param name="distanceKm">The computed distance from the donor to the request.</param>
    /// <param name="smsSent">Whether an SMS fallback was also dispatched for this notification.</param>
    /// <param name="createdAtUtc">UTC timestamp the notification is created.</param>
    public static DonorNotification Create(
        BloodRequest bloodRequest,
        Guid donorProfileId,
        decimal distanceKm,
        bool smsSent,
        DateTimeOffset createdAtUtc)
    {
        return new DonorNotification
        {
            BloodRequestId = bloodRequest.Id,
            DonorProfileId = donorProfileId,
            BloodGroup = bloodRequest.BloodGroup,
            UnitsRequired = bloodRequest.UnitsRequired,
            Urgency = bloodRequest.Urgency,
            DistanceKm = distanceKm,
            AreaLabel = bloodRequest.LocationCityArea,
            IsRead = false,
            SmsSent = smsSent,
            CreatedAtUtc = createdAtUtc
        };
    }
}
