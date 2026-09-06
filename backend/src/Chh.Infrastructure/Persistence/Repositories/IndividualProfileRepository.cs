using Chh.Application.Contracts;
using Chh.Domain.Entities;
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
    public async Task AddAsync(IndividualProfile individualProfile, CancellationToken ct) =>
        await _context.IndividualProfiles.AddAsync(individualProfile, ct).ConfigureAwait(false);
}
