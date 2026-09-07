using Chh.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Chh.Infrastructure.Persistence.Repositories;

/// <summary>EF Core-backed implementation of <see cref="IAdminUserRepository"/>.</summary>
public class AdminUserRepository : IAdminUserRepository
{
    private readonly ChhDbContext _context;

    /// <summary>Creates the repository with the shared <see cref="ChhDbContext"/>.</summary>
    /// <param name="context">The shared EF Core database context.</param>
    public AdminUserRepository(ChhDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<bool> IsAdminAsync(string mobileNumber, CancellationToken ct)
    {
        return await _context.AdminUsers
            .AsNoTracking()
            .AnyAsync(a => a.MobileNumber == mobileNumber && a.IsAdmin, ct)
            .ConfigureAwait(false);
    }
}
