using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Logic layer for facility registration (CHH-78) — distinct from <see cref="IFacilityAdminService"/>'s moderation concern.</summary>
public interface IFacilityService
{
    /// <summary>Registers a new facility (hospital/blood-bank or NGO), pending System Admin verification.</summary>
    /// <param name="request">The registration details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Chh.Application.Abstractions.FacilityAlreadyRegisteredException">A facility already exists for this license number.</exception>
    Task<FacilityDto> RegisterAsync(CreateFacilityRequest request, CancellationToken ct);

    /// <summary>
    /// Returns the facility owned by <paramref name="mobileNumber"/> (CHH-28's status dashboard),
    /// or <c>null</c> if that mobile number isn't a contact on any facility.
    /// </summary>
    /// <param name="mobileNumber">The authenticated caller's mobile number (from the JWT "sub" claim).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<FacilityDto?> GetMyFacilityAsync(string mobileNumber, CancellationToken ct);

    /// <summary>
    /// Uploads and attaches a license document to a facility (CHH-79/US-CHH-003-02), replacing
    /// any previously-uploaded document's URL. Returns <c>null</c> if no facility exists for
    /// <paramref name="facilityId"/>.
    /// </summary>
    /// <param name="facilityId">The facility the document belongs to.</param>
    /// <param name="content">The file content stream.</param>
    /// <param name="fileName">The uploaded file's original name.</param>
    /// <param name="contentType">The file's MIME type.</param>
    /// <param name="contentLength">The file's size in bytes.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Abstractions.InvalidFacilityDocumentException">The file's type isn't allowed, it's too large, or none was provided.</exception>
    Task<FacilityDto?> UploadLicenseDocumentAsync(
        Guid facilityId, Stream content, string fileName, string contentType, long contentLength, CancellationToken ct);
}
