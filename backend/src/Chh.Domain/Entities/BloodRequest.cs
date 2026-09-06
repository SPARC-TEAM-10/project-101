using Chh.Domain.Enums;

namespace Chh.Domain.Entities;

/// <summary>
/// A blood request with a search radius for proximity donor matching (CHH-33/US-CHH-004-01, part
/// of Epic CHH-25 — CHH-F04 Proximity Notifications). A plain property bag — construction logic
/// lives in <see cref="Chh.Application.Factories.BloodRequestFactory"/>, matching the pattern
/// established for <see cref="IndividualProfile"/> on CHH-F02.
/// </summary>
public class BloodRequest
{
    /// <summary>Surrogate primary key.</summary>
    public Guid Id { get; internal set; } = Guid.NewGuid();

    /// <summary>
    /// Mobile number of the requester, taken from the JWT "sub" claim at creation time — never
    /// client-supplied (see <c>Chh.Api.Controllers.BloodRequestsController</c>).
    /// </summary>
    public string RequesterMobileNumber { get; internal set; } = default!;

    /// <summary>Patient's name (AC1/AC4 mandatory field).</summary>
    public string PatientName { get; internal set; } = default!;

    /// <summary>Required blood group (AC1/AC4 mandatory field).</summary>
    public BloodGroup BloodGroup { get; internal set; }

    /// <summary>Number of units required, must be greater than zero (AC1/AC4, Edge Case: "Units required set to 0").</summary>
    public int UnitsRequired { get; internal set; }

    /// <summary>City/area, free text (AC1/AC4 mandatory field) — same simplification as <see cref="IndividualProfile.LocationCityArea"/>.</summary>
    public string LocationCityArea { get; internal set; } = default!;

    /// <summary>
    /// Latitude captured from the requester's device at submission time (browser/device
    /// Geolocation, not a paid maps/geocoding API — see CreateBloodRequestRequestValidator's
    /// remarks on why). Required; a missing value is the Edge Case "location coordinates cannot
    /// be resolved", surfaced as a validation failure before an entity is ever constructed.
    /// </summary>
    public decimal Latitude { get; internal set; }

    /// <summary>Longitude captured from the requester's device at submission time — see <see cref="Latitude"/>.</summary>
    public decimal Longitude { get; internal set; }

    /// <summary>Search radius in kilometers, 5–100 inclusive (AC2/AC3).</summary>
    public int SearchRadiusKm { get; internal set; }

    /// <summary>Urgency of the request.</summary>
    public UrgencyLevel Urgency { get; internal set; }

    /// <summary>Lifecycle state — <see cref="BloodRequestStatus.Matching"/> from creation (AC1).</summary>
    public BloodRequestStatus Status { get; internal set; }

    /// <summary>UTC timestamp the request was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; internal set; }

    /// <summary>UTC timestamp the request auto-expires — <see cref="CreatedAtUtc"/> plus the 6-hour validity window.</summary>
    public DateTimeOffset ExpiresAtUtc { get; internal set; }
}
