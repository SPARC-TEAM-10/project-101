using Chh.Application.Dtos;

namespace Chh.Application.Contracts;

/// <summary>Records a donor's accept/decline response to their own notification (CHH-35/US-CHH-004-04).</summary>
public interface IDonorResponseService
{
    /// <summary>
    /// Accepts the matched blood request on the caller's behalf (AC1). Returns <c>null</c> if the
    /// notification doesn't exist, belongs to a different donor, or the caller hasn't registered
    /// an individual profile yet.
    /// </summary>
    /// <param name="mobileNumber">The authenticated caller's mobile number, from the JWT.</param>
    /// <param name="notificationId">The notification being responded to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Abstractions.DonorAlreadyRespondedException">The donor already accepted or declined this notification.</exception>
    /// <exception cref="Abstractions.BloodRequestNoLongerActiveException">The request is expired, already fulfilled, or its last remaining unit was just taken by another donor (AC3/Edge Case).</exception>
    Task<DonorResponseResultDto?> AcceptAsync(string mobileNumber, Guid notificationId, CancellationToken ct);

    /// <summary>
    /// Declines the matched blood request on the caller's behalf (AC2). Returns <c>null</c> if the
    /// notification doesn't exist, belongs to a different donor, or the caller hasn't registered
    /// an individual profile yet.
    /// </summary>
    /// <param name="mobileNumber">The authenticated caller's mobile number, from the JWT.</param>
    /// <param name="notificationId">The notification being responded to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Abstractions.DonorAlreadyRespondedException">The donor already accepted or declined this notification.</exception>
    Task<DonorResponseResultDto?> DeclineAsync(string mobileNumber, Guid notificationId, CancellationToken ct);
}
