namespace Chh.Domain.Enums;

/// <summary>Lifecycle state of an <see cref="Entities.Event"/> (CHH-38/CHH-41).</summary>
public enum EventStatus
{
    /// <summary>Live and discoverable — the only state CHH-38 (creation) ever produces.</summary>
    Published = 1,

    /// <summary>Cancelled by the organizing facility (CHH-41) — removed from public discovery.</summary>
    Cancelled = 2
}
