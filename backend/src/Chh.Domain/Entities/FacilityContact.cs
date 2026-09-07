namespace Chh.Domain.Entities;

/// <summary>A contact person for a <see cref="Facility"/> (CHH-78 AC2).</summary>
public class FacilityContact
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Owning facility.</summary>
    public Guid FacilityId { get; set; }

    /// <summary>Contact's full name.</summary>
    public string Name { get; set; } = default!;

    /// <summary>Contact's designation/role at the facility.</summary>
    public string Designation { get; set; } = default!;

    /// <summary>Contact's 10-digit mobile number.</summary>
    public string Mobile { get; set; } = default!;
}
