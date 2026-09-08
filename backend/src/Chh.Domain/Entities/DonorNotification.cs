using Chh.Domain.Enums;

namespace Chh.Domain.Entities;

/// <summary>
/// A single donor's notification for a matched blood request (CHH-34/US-CHH-004-03, part of Epic
/// CHH-25 — CHH-F04 Proximity Notifications). Created by <c>Chh.Application.Jobs.MatchDonorsJob</c>
/// right after the matching engine (CHH-80) finds an eligible donor. Deliberately excludes the
/// patient's name and exact address (AC2/business rules) — only the four off-app-safe facts:
/// blood group, units, urgency, and approximate distance/area. A plain property bag — construction
/// lives in <see cref="Chh.Application.Factories.DonorNotificationFactory"/>, matching the pattern
/// established for <see cref="IndividualProfile"/> on CHH-F02.
/// </summary>
public class DonorNotification
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; internal set; } = Guid.NewGuid();

    /// <summary>The blood request this notification is for.</summary>
    public Guid BloodRequestId { get; internal set; }

    /// <summary>The notified donor's <c>IndividualProfile</c> id.</summary>
    public Guid DonorProfileId { get; internal set; }

    /// <summary>Blood group requested (AC2) — denormalized copy at dispatch time.</summary>
    public BloodGroup BloodGroup { get; internal set; }

    /// <summary>Units required (AC2) — denormalized copy at dispatch time.</summary>
    public int UnitsRequired { get; internal set; }

    /// <summary>Urgency (AC2) — denormalized copy at dispatch time.</summary>
    public UrgencyLevel Urgency { get; internal set; }

    /// <summary>Approximate distance from the donor to the request, in kilometers (AC2 — "5km away").</summary>
    public decimal DistanceKm { get; internal set; }

    /// <summary>
    /// The request's city/area (AC2's "approximate distance" companion context) — never the exact
    /// address, per the off-app-delivery business rule.
    /// </summary>
    public string AreaLabel { get; internal set; } = default!;

    /// <summary>Read status (AC3 — "new request should appear at the top with an 'Unread' status").</summary>
    public bool IsRead { get; internal set; }

    /// <summary>
    /// True if an SMS was also dispatched for this notification (donor was not "currently active"
    /// in-app at match time — see <c>Chh.Application.Services.NotificationDispatchService</c>).
    /// </summary>
    public bool SmsSent { get; internal set; }

    /// <summary>UTC timestamp the notification was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; internal set; }
}
