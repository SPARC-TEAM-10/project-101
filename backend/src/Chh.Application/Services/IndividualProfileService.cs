using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Factories;

namespace Chh.Application.Services;

/// <summary>Orchestrates individual registration: verification guard, uniqueness, persistence (CHH-F02).</summary>
public class IndividualProfileService : IIndividualProfileService
{
    private readonly IIndividualProfileRepository _individualProfileRepository;
    private readonly IOtpRequestRepository _otpRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its repository and unit-of-work dependencies.</summary>
    /// <param name="individualProfileRepository">Data layer for reading and persisting individual profiles.</param>
    /// <param name="otpRequestRepository">Data layer used to confirm the mobile number has a verified OTP.</param>
    /// <param name="unitOfWork">Persists changes made during the request.</param>
    public IndividualProfileService(
        IIndividualProfileRepository individualProfileRepository,
        IOtpRequestRepository otpRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _individualProfileRepository = individualProfileRepository;
        _otpRequestRepository = otpRequestRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<IndividualProfileDto> RegisterAsync(CreateIndividualProfileRequest request, CancellationToken ct)
    {
        var latestOtp = await _otpRequestRepository
            .GetLatestByMobileNumberAsync(request.MobileNumber, ct)
            .ConfigureAwait(false);

        if (latestOtp is null || !latestOtp.IsVerified)
        {
            throw new MobileNumberNotVerifiedException();
        }

        var existingProfile = await _individualProfileRepository
            .GetByMobileNumberAsync(request.MobileNumber, ct)
            .ConfigureAwait(false);

        if (existingProfile is not null)
        {
            throw new IndividualAlreadyRegisteredException();
        }

        var profile = IndividualProfileFactory.Create(request, DateTimeOffset.UtcNow);

        await _individualProfileRepository.AddAsync(profile, ct).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new IndividualProfileDto
        {
            Id = profile.Id,
            FullName = profile.FullName,
            BloodGroup = profile.BloodGroup,
            IsReceiverOnly = profile.IsReceiverOnly,
            CreatedAtUtc = profile.CreatedAtUtc
        };
    }
}
