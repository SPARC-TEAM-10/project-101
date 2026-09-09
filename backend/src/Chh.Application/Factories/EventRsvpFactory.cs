using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Factories;

/// <summary>Constructs <see cref="EventRsvp"/> instances, matching <see cref="EventFactory"/>'s established pattern.</summary>
public static class EventRsvpFactory
{
    /// <summary>Creates a new active (<see cref="EventRsvpStatus.Going"/>) RSVP row.</summary>
    /// <param name="eventId">The event being RSVP'd to.</param>
    /// <param name="individualProfileId">The RSVP'ing individual.</param>
    /// <param name="referenceCode">The assigned reference code (see <see cref="EventRsvp.ReferenceCode"/>'s doc comment).</param>
    /// <param name="createdAtUtc">UTC timestamp the RSVP is created.</param>
    public static EventRsvp Create(Guid eventId, Guid individualProfileId, string referenceCode, DateTimeOffset createdAtUtc)
    {
        return new EventRsvp
        {
            EventId = eventId,
            IndividualProfileId = individualProfileId,
            ReferenceCode = referenceCode,
            Status = EventRsvpStatus.Going,
            CreatedAtUtc = createdAtUtc
        };
    }
}
