using Chh.Domain.Enums;

namespace Chh.Domain.Constants;

/// <summary>
/// Human-readable label ("blood donation camp", etc.) for <see cref="EventType"/> — used in SMS
/// copy (CHH-41's edit/cancel notifications), matching EventSmsPreview.dc.html's lowercase phrasing.
/// </summary>
public static class EventTypeDisplay
{
    private static readonly IReadOnlyDictionary<EventType, string> Labels = new Dictionary<EventType, string>
    {
        [EventType.BloodDonationCamp] = "blood donation camp",
        [EventType.HealthCamp] = "health camp",
        [EventType.AwarenessSession] = "awareness session",
        [EventType.Other] = "event"
    };

    /// <summary>Returns the lowercase display label for <paramref name="eventType"/> (e.g. "blood donation camp").</summary>
    public static string ToLabel(EventType eventType) => Labels[eventType];
}
