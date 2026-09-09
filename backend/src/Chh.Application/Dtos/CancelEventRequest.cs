namespace Chh.Application.Dtos;

/// <summary>Request body for <c>POST /api/v1/events/{id}/cancel</c> (CHH-41/US-CHH-005-04 AC1).</summary>
public record CancelEventRequest
{
    /// <summary>
    /// Shown verbatim to RSVP'd attendees (EventEditWeb.dc.html's cancel modal label: "Attendees
    /// see this word for word").
    /// </summary>
    public required string Reason { get; init; }
}
