using Chh.Application.Contracts;
using Chh.Domain.Entities;

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
}
