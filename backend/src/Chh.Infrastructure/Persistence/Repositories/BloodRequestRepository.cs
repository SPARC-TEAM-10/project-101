using Chh.Application.Contracts;
using Chh.Domain.Entities;
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
}
