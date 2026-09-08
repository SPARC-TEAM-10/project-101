namespace Chh.Domain.Enums;

/// <summary>Lifecycle state of a <c>BloodRequest</c> (CHH-33/US-CHH-004-01, CHH-35/US-CHH-004-04).</summary>
public enum BloodRequestStatus
{
    /// <summary>Actively searching for eligible donors within the search radius.</summary>
    Matching = 1,

    /// <summary>Past its 6-hour window with no fulfillment.</summary>
    Expired = 2,

    /// <summary>
    /// Enough donors have accepted to cover <c>UnitsRequired</c> (CHH-35 AC1/Edge Case). Terminal —
    /// no further accepts are recorded once reached (see <c>DonorResponseService</c>).
    /// </summary>
    Fulfilled = 3
}
