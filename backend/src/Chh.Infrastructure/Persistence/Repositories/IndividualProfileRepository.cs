using Chh.Application.Contracts;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Chh.Infrastructure.Persistence.Repositories;

/// <summary>EF Core-backed implementation of <see cref="IIndividualProfileRepository"/>.</summary>
public class IndividualProfileRepository : IIndividualProfileRepository
{
    private readonly ChhDbContext _context;

    /// <summary>Creates the repository with the shared <see cref="ChhDbContext"/>.</summary>
    /// <param name="context">The shared EF Core database context.</param>
    public IndividualProfileRepository(ChhDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IndividualProfile?> GetByMobileNumberAsync(string mobileNumber, CancellationToken ct) =>
        await _context.IndividualProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.MobileNumber == mobileNumber, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IndividualProfile?> GetTrackedByMobileNumberAsync(string mobileNumber, CancellationToken ct) =>
        await _context.IndividualProfiles
            .FirstOrDefaultAsync(p => p.MobileNumber == mobileNumber, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task AddAsync(IndividualProfile individualProfile, CancellationToken ct) =>
        await _context.IndividualProfiles.AddAsync(individualProfile, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IReadOnlyList<IndividualProfile>> GetActiveDonorsByBloodGroupsAsync(IReadOnlySet<BloodGroup> bloodGroups, CancellationToken ct) =>
        await _context.IndividualProfiles
            .AsNoTracking()
            .Where(p => bloodGroups.Contains(p.BloodGroup)
                && p.AccountStatus == AccountStatus.Active
                && !p.IsReceiverOnly
                && p.Latitude != null
                && p.Longitude != null)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<IndividualProfile?> GetTrackedByIdAsync(Guid id, CancellationToken ct) =>
        await _context.IndividualProfiles
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<IndividualProfile> Items, int TotalCount)> SearchAsync(
        string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.IndividualProfiles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(p => p.MobileNumber.Contains(term) || p.FullName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct).ConfigureAwait(false);

        var items = await query
            .OrderBy(p => p.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IndividualProfile>> GetActiveWithKnownLocationAsync(CancellationToken ct) =>
        await _context.IndividualProfiles
            .AsNoTracking()
            .Where(p => p.AccountStatus == AccountStatus.Active && p.Latitude != null && p.Longitude != null)
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
