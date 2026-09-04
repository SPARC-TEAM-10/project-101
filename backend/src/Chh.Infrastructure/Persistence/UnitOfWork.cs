using Chh.Application.Contracts;

namespace Chh.Infrastructure.Persistence;

/// <summary>EF Core-backed implementation of <see cref="IUnitOfWork"/> over the shared <see cref="ChhDbContext"/>.</summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ChhDbContext _context;

    /// <summary>Creates the unit of work with the shared <see cref="ChhDbContext"/>.</summary>
    /// <param name="context">The shared EF Core database context.</param>
    public UnitOfWork(ChhDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync(CancellationToken ct) =>
        await _context.SaveChangesAsync(ct).ConfigureAwait(false);
}
