namespace Chh.Application.Dtos;

/// <summary>Contact person on a <c>FacilityDto</c> (CHH-73).</summary>
public record FacilityContactDto
{
    /// <summary>Contact's full name.</summary>
    public required string Name { get; init; }

    /// <summary>Contact's designation/role at the facility.</summary>
    public required string Designation { get; init; }

    /// <summary>Contact's 10-digit mobile number.</summary>
    public required string Mobile { get; init; }
}
