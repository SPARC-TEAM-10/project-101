using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chh.Infrastructure.Persistence.Repositories;

/// <summary>EF Core-backed implementation of <see cref="IDonorNotificationRepository"/>.</summary>
public class DonorNotificationRepository : IDonorNotificationRepository
{
    private readonly ChhDbContext _context;

    /// <summary>Creates the repository with the shared <see cref="ChhDbContext"/>.</summary>
    /// <param name="context">The shared EF Core database context.</param>
    public DonorNotificationRepository(ChhDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(Guid bloodRequestId, Guid donorProfileId, CancellationToken ct) =>
        await _context.DonorNotifications
            .AsNoTracking()
            .AnyAsync(n => n.BloodRequestId == bloodRequestId && n.DonorProfileId == donorProfileId, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task AddAsync(DonorNotification notification, CancellationToken ct) =>
        await _context.DonorNotifications.AddAsync(notification, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<DonorNotification> Items, int TotalCount)> GetByDonorAsync(
        Guid donorProfileId, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.DonorNotifications
            .AsNoTracking()
            .Where(n => n.DonorProfileId == donorProfileId);

        var totalCount = await query.CountAsync(ct).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<DonorNotification?> GetTrackedByIdForDonorAsync(Guid id, Guid donorProfileId, CancellationToken ct) =>
        await _context.DonorNotifications
            .FirstOrDefaultAsync(n => n.Id == id && n.DonorProfileId == donorProfileId, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IReadOnlyList<DonorNotificationWithDonorInfo>> GetWithDonorInfoByBloodRequestIdAsync(Guid bloodRequestId, CancellationToken ct) =>
        await _context.DonorNotifications
            .AsNoTracking()
            .Where(n => n.BloodRequestId == bloodRequestId)
            .OrderBy(n => n.CreatedAtUtc)
            .Join(
                _context.IndividualProfiles.AsNoTracking(),
                notification => notification.DonorProfileId,
                profile => profile.Id,
                (notification, profile) => new DonorNotificationWithDonorInfo
                {
                    DonorProfileId = notification.DonorProfileId,
                    ResponseStatus = notification.ResponseStatus,
                    IsRead = notification.IsRead,
                    CreatedAtUtc = notification.CreatedAtUtc,
                    DonorFullName = profile.FullName,
                    DonorMobileNumber = profile.MobileNumber
                })
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
