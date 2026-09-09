namespace Chh.Domain.Constants;

/// <summary>
/// License-document-upload-related constants (CHH-79/US-CHH-003-02) — mirrors the frontend's own
/// copy of these rules in <c>frontend/src/lib/validation/facilityUploadValidation.ts</c>, since
/// client-side validation is never trusted alone (api-standards.md §5).
/// </summary>
public static class FacilityDocumentConstants
{
    /// <summary>Allowed MIME types for an uploaded license document (AC1 — "PDF or Image").</summary>
    public static readonly IReadOnlySet<string> AllowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };

    /// <summary>Maximum allowed file size, in bytes (AC3 — 5MB).</summary>
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    /// <summary>Validation message when the file's content type isn't in <see cref="AllowedContentTypes"/>.</summary>
    public const string InvalidFileTypeMessage = "Invalid file format. Please upload PDF or Image.";

    /// <summary>Validation message when the file exceeds <see cref="MaxFileSizeBytes"/>.</summary>
    public const string FileTooLargeMessage = "File too large. Maximum size is 5MB.";

    /// <summary>Validation message when no file was included in the request.</summary>
    public const string NoFileProvidedMessage = "Please attach a file to upload.";
}
