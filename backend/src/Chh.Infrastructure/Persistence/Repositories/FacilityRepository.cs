using Chh.Application.Contracts;
using Chh.Domain.Entities;

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
}
