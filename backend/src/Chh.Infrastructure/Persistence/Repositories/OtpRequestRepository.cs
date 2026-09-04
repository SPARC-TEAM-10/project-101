using Chh.Application.Contracts;
using Chh.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chh.Infrastructure.Persistence.Repositories;

/// <summary>EF Core-backed implementation of <see cref="IOtpRequestRepository"/>.</summary>
public class OtpRequestRepository : IOtpRequestRepository
{
    private readonly ChhDbContext _context;

    /// <summary>Creates the repository with the shared <see cref="ChhDbContext"/>.</summary>
    /// <param name="context">The shared EF Core database context.</param>
    public OtpRequestRepository(ChhDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<OtpRequest?> GetLatestByMobileNumberAsync(string mobileNumber, CancellationToken ct) =>
        await _context.OtpRequests
            .AsNoTracking()
            .Where(o => o.MobileNumber == mobileNumber)
            .OrderByDescending(o => o.OtpRequestedAtUtc)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task AddAsync(OtpRequest otpRequest, CancellationToken ct) =>
        await _context.OtpRequests.AddAsync(otpRequest, ct).ConfigureAwait(false);
}
