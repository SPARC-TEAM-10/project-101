namespace Chh.Domain.Constants;

/// <summary>Blood-request-related constants (CHH-33/US-CHH-004-01) — radius bounds, expiry window, user-facing messages.</summary>
public static class BloodRequestConstants
{
    /// <summary>Minimum allowed search radius, in kilometers (AC2).</summary>
    public const int MinSearchRadiusKm = 5;

    /// <summary>Maximum allowed search radius, in kilometers (AC3).</summary>
    public const int MaxSearchRadiusKm = 100;

    /// <summary>How long a request stays open before auto-expiry (business rule).</summary>
    public static readonly TimeSpan RequestValidity = TimeSpan.FromHours(6);

    /// <summary>Validation message when the radius is below <see cref="MinSearchRadiusKm"/> (AC2).</summary>
    public const string RadiusTooSmallMessage = "Minimum radius is 5km";

    /// <summary>Validation message when the radius is above <see cref="MaxSearchRadiusKm"/> (AC3).</summary>
    public const string RadiusTooLargeMessage = "Maximum radius is 100km";

    /// <summary>Validation message when the requester's location coordinates weren't provided/resolvable (Edge Case).</summary>
    public const string LocationNotResolvableMessage =
        "Could not determine your location. Please enable location access and try again.";
}
