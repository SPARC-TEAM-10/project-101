namespace Chh.Infrastructure.Storage;

/// <summary>Bound from the "FacilityDocumentStorage" configuration section.</summary>
public class FacilityDocumentStorageOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "FacilityDocumentStorage";

    /// <summary>
    /// Absolute or app-relative filesystem path documents are written under. Real cloud blob
    /// storage (e.g. S3, matching the AWS backend deployment target — root CLAUDE.md Decisions
    /// Log 2026-09-05) is an open infra decision, same status as the SMS gateway provider was
    /// before Fast2SMS was chosen (see backend/CLAUDE.md's Tech Stack table) — this local-disk
    /// implementation keeps CHH-79/CHH-74 unblocked in the meantime. Swap
    /// <see cref="LocalDiskFacilityDocumentStorageService"/> for a real blob-storage
    /// implementation of <c>IFacilityDocumentStorageService</c> once that decision is made; no
    /// other layer needs to change.
    /// </summary>
    public string RootPath { get; set; } = "uploads/facility-documents";

    /// <summary>URL path prefix the stored files are served under (see <c>Program.cs</c>'s <c>UseStaticFiles</c>).</summary>
    public string UrlPrefix { get; set; } = "/uploads/facility-documents";
}
