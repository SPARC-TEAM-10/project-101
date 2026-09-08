namespace Chh.Domain.Constants;

/// <summary>Thresholds for CHH-34's "is this donor currently active in-app" presence check.</summary>
public static class PresenceConstants
{
    /// <summary>
    /// A donor whose <c>IndividualProfile.LastActiveAtUtc</c> falls within this window of "now" is
    /// considered currently active in-app — the SMS fallback is skipped for them.
    /// </summary>
    public static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(2);

    /// <summary>
    /// <c>ActivityTrackingMiddleware</c> only writes <c>LastActiveAtUtc</c> when the stored value is
    /// older than this, to avoid a database write on every authenticated request.
    /// </summary>
    public static readonly TimeSpan UpdateThrottle = TimeSpan.FromSeconds(60);
}
