namespace Chh.Application.Abstractions;

// Manual attendance marking domain exceptions (CHH-44/US-CHH-005-07).

/// <summary>Raised when the participant has already been marked attended (duplicate prevention, §6.1). Maps to 409 Conflict.</summary>
public class AlreadyAttendedException : ChhException
{
    /// <summary>Creates the exception with the standard already-attended message.</summary>
    public AlreadyAttendedException()
        : base(Chh.Domain.Constants.EventConstants.AlreadyAttendedMessage)
    {
    }
}

/// <summary>
/// Raised when marking attendance outside the approved check-in window (spec §6.1: "1 hour
/// before event start until event end"). Maps to 422 Unprocessable Entity.
/// </summary>
public class AttendanceOutsideWindowException : ChhException
{
    /// <summary>Creates the exception with the standard outside-window message.</summary>
    public AttendanceOutsideWindowException()
        : base(Chh.Domain.Constants.EventConstants.AttendanceOutsideWindowMessage)
    {
    }
}

/// <summary>
/// Raised when attempting to mark attendance for an RSVP that isn't <see cref="Chh.Domain.Enums.EventRsvpStatus.Going"/>
/// (i.e. it was cancelled). Maps to 422 Unprocessable Entity.
/// </summary>
public class RsvpNotEligibleForAttendanceException : ChhException
{
    /// <summary>Creates the exception with the standard not-eligible message.</summary>
    public RsvpNotEligibleForAttendanceException()
        : base(Chh.Domain.Constants.EventConstants.RsvpNotEligibleForAttendanceMessage)
    {
    }
}
