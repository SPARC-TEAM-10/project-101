using Chh.Application.Contracts;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Chh.Infrastructure.Persistence.Repositories;

/// <summary>EF Core-backed implementation of <see cref="IFacilityRepository"/>.</summary>
public class FacilityRepository : IFacilityRepository
{
    private readonly ChhDbContext _context;

    /// <summary>Creates the repository with the shared <see cref="ChhDbContext"/>.</summary>
    /// <param name="context">The shared EF Core database context.</param>
    public FacilityRepository(ChhDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(Facility facility, CancellationToken ct) =>
        await _context.Facilities.AddAsync(facility, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<(IReadOnlyList<Facility> Items, int TotalCount)> GetByStatusAsync(
        FacilityVerificationStatus status, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Facilities
            .AsNoTracking()
            .Include(f => f.Contacts)
            .Where(f => f.VerificationStatus == status);

        var totalCount = await query.CountAsync(ct).ConfigureAwait(false);

        var items = await query
            .OrderBy(f => f.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return (items, totalCount);
    }

    /// <inheritdoc />
    public async Task<Facility?> GetByContactMobileNumberAsync(string mobileNumber, CancellationToken ct) =>
        await _context.Facilities
            .AsNoTracking()
            .Include(f => f.Contacts)
            .FirstOrDefaultAsync(f => f.Contacts.Any(c => c.Mobile == mobileNumber), ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Facility?> GetByLicenseNumberAsync(string licenseNumber, CancellationToken ct) =>
        await _context.Facilities
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.LicenseNumber == licenseNumber, ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<Facility?> GetTrackedByIdAsync(Guid id, CancellationToken ct) =>
        await _context.Facilities
            .Include(f => f.Contacts)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            .ConfigureAwait(false);
}
