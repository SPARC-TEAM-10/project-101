namespace Chh.Domain.Entities;

/// <summary>
/// A point of contact for a <see cref="Facility"/> (CHH-78/US-CHH-003-01 AC2). 1 to 3 per facility;
/// a donor responding to a request from this facility sees the primary contact's (lowest
/// <see cref="SortOrder"/>) name and number (CHH-F03 LLD §6.9).
/// </summary>
public class FacilityContact
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; internal set; } = Guid.NewGuid();

    /// <summary>Foreign key to the owning <see cref="Facility"/>.</summary>
    public Guid FacilityId { get; internal set; }

    /// <summary>Contact's full name (AC2 mandatory field).</summary>
    public string Name { get; internal set; } = default!;

    /// <summary>Contact's role at the facility, e.g. "Blood bank officer" (AC2 mandatory field).</summary>
    public string Designation { get; internal set; } = default!;

    /// <summary>Contact's mobile number, 10 digits (AC2 mandatory field). Unique within the facility.</summary>
    public string Mobile { get; internal set; } = default!;

    /// <summary>1-based position among this facility's contacts — 1 is the primary contact.</summary>
    public int SortOrder { get; internal set; }
}
