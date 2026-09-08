using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Constants;
using Chh.Domain.Entities;
using Chh.Domain.Enums;

namespace Chh.Application.Services;

/// <summary>Orchestrates System Admin user moderation (CHH-76/US-CHH-001-04, Admin Command Center).</summary>
public class UserAdminService : IUserAdminService
{
    private readonly IIndividualProfileRepository _individualProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>Creates the service with its repository and unit-of-work dependencies.</summary>
    /// <param name="individualProfileRepository">Data layer for reading and updating individual profiles.</param>
    /// <param name="unitOfWork">Persists the suspend action.</param>
    public UserAdminService(IIndividualProfileRepository individualProfileRepository, IUnitOfWork unitOfWork)
    {
        _individualProfileRepository = individualProfileRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public async Task<PagedResponse<AdminUserDto>> SearchUsersAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1
            ? PaginationConstants.DefaultPageSize
            : Math.Min(pageSize, PaginationConstants.MaxPageSize);

        var (profiles, totalCount) = await _individualProfileRepository
            .SearchAsync(search, normalizedPage, normalizedPageSize, ct)
            .ConfigureAwait(false);

        return new PagedResponse<AdminUserDto>
        {
            Items = profiles.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = normalizedPage,
            PageSize = normalizedPageSize
        };
    }

    /// <inheritdoc />
    public async Task<AdminUserDto?> SuspendUserAsync(Guid userId, string reason, CancellationToken ct)
    {
        var profile = await _individualProfileRepository.GetTrackedByIdAsync(userId, ct).ConfigureAwait(false);
        if (profile is null)
        {
            return null;
        }

        profile.AccountStatus = AccountStatus.Suspended;
        profile.SuspensionReason = reason;

        await _unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return MapToDto(profile);
    }

    private static AdminUserDto MapToDto(IndividualProfile p) => new()
    {
        Id = p.Id,
        MobileNumber = p.MobileNumber,
        FullName = p.FullName,
        BloodGroup = p.BloodGroup,
        AccountStatus = p.AccountStatus,
        SuspensionReason = p.SuspensionReason,
        CreatedAtUtc = p.CreatedAtUtc
    };
}
