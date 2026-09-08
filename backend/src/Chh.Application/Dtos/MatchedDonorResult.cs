namespace Chh.Application.Dtos;

/// <summary>An eligible donor found by the matching engine (US-CHH-004-02/CHH-80), with computed distance.</summary>
public record MatchedDonorResult
{
    /// <summary>The matched donor's <c>IndividualProfile</c> id.</summary>
    public required Guid DonorProfileId { get; init; }

    /// <summary>The matched donor's mobile number, used by CHH-34's notification dispatch.</summary>
    public required string MobileNumber { get; init; }

    /// <summary>Distance from the request's location to the donor's registered location, in kilometers.</summary>
    public required decimal DistanceKm { get; init; }
}
