namespace Chh.Application.Dtos;

/// <summary>One contact person on a <see cref="CreateFacilityRequest"/> (CHH-78 AC2).</summary>
public record CreateFacilityContactRequest
{
    /// <summary>Contact's full name.</summary>
    public required string Name { get; init; }

    /// <summary>Contact's designation/role at the facility.</summary>
    public required string Designation { get; init; }

    /// <summary>Contact's 10-digit mobile number.</summary>
    public required string Mobile { get; init; }
}
