using Chh.Application.Contracts;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Chh.Infrastructure.Persistence.Repositories;

/// <summary>EF Core-backed implementation of <see cref="IBloodRequestRepository"/>.</summary>
public class BloodRequestRepository : IBloodRequestRepository
{
    private readonly ChhDbContext _context;

    /// <summary>Creates the repository with the shared <see cref="ChhDbContext"/>.</summary>
    /// <param name="context">The shared EF Core database context.</param>
    public BloodRequestRepository(ChhDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(BloodRequest bloodRequest, CancellationToken ct) =>
        await _context.BloodRequests.AddAsync(bloodRequest, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<BloodRequest?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _context.BloodRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<BloodRequest> Items, int TotalCount)> GetByRequesterAsync(
        string requesterMobileNumber, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.BloodRequests
            .AsNoTracking()
            .Where(r => r.RequesterMobileNumber == requesterMobileNumber);

        var totalCount = await query.CountAsync(ct).ConfigureAwait(false);

        var items = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<bool> TryAcceptUnitAsync(Guid bloodRequestId, DateTimeOffset nowUtc, CancellationToken ct)
    {
        var rowsAffected = await _context.BloodRequests
            .Where(r => r.Id == bloodRequestId
                && r.Status == BloodRequestStatus.Matching
                && r.ExpiresAtUtc > nowUtc
                && r.UnitsAccepted < r.UnitsRequired)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(r => r.UnitsAccepted, r => r.UnitsAccepted + 1)
                    .SetProperty(r => r.Status, r => r.UnitsAccepted + 1 >= r.UnitsRequired ? BloodRequestStatus.Fulfilled : r.Status),
                ct)
            .ConfigureAwait(false);

        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<BloodRequest?> GetTrackedByIdAsync(Guid id, CancellationToken ct) =>
        await _context.BloodRequests
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            .ConfigureAwait(false);
}
