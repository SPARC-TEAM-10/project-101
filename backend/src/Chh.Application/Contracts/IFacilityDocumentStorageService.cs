namespace Chh.Application.Contracts;

/// <summary>
/// Stores an uploaded facility license document and returns a URL the frontend can later use to
/// view it (CHH-74 AC1 — "open the uploaded PDF or Image in a new tab or modal"). Per
/// `.claude/rules/db-standards.md` §3: license documents live in blob storage, never inline
/// `bytea` — the DB only ever holds the returned URL (<c>Facility.LicenseDocumentUrl</c>).
/// </summary>
public interface IFacilityDocumentStorageService
{
    /// <summary>
    /// Saves <paramref name="content"/> under a path unique to <paramref name="facilityId"/> and
    /// returns the URL it can be retrieved from.
    /// </summary>
    /// <param name="facilityId">The facility the document belongs to.</param>
    /// <param name="fileName">The uploaded file's original name — used only to preserve the extension.</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="contentType">The file's MIME type.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string> SaveAsync(Guid facilityId, string fileName, Stream content, string contentType, CancellationToken ct);
}
