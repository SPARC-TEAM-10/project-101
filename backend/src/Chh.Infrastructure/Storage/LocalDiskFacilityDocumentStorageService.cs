using Chh.Application.Contracts;
using Microsoft.Extensions.Options;

namespace Chh.Infrastructure.Storage;

/// <summary>
/// Local-disk-backed <see cref="IFacilityDocumentStorageService"/> — see
/// <see cref="FacilityDocumentStorageOptions.RootPath"/> for why this isn't real blob storage yet.
/// Files are written under <c>{RootPath}/{facilityId}/{new Guid}{extension}</c> so two uploads for
/// the same facility (e.g. a re-upload) never collide, and served back via <c>Program.cs</c>'s
/// <c>UseStaticFiles</c> mapped at <see cref="FacilityDocumentStorageOptions.UrlPrefix"/>.
/// </summary>
public class LocalDiskFacilityDocumentStorageService : IFacilityDocumentStorageService
{
    private readonly FacilityDocumentStorageOptions _options;

    /// <summary>Creates the service with its bound options.</summary>
    /// <param name="options">Bound "FacilityDocumentStorage" configuration.</param>
    public LocalDiskFacilityDocumentStorageService(IOptions<FacilityDocumentStorageOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<string> SaveAsync(Guid facilityId, string fileName, Stream content, string contentType, CancellationToken ct)
    {
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";

        var facilityDirectory = Path.Combine(_options.RootPath, facilityId.ToString());
        Directory.CreateDirectory(facilityDirectory);

        var absolutePath = Path.Combine(facilityDirectory, storedFileName);
        await using (var fileStream = File.Create(absolutePath))
        {
            await content.CopyToAsync(fileStream, ct).ConfigureAwait(false);
        }

        return $"{_options.UrlPrefix}/{facilityId}/{storedFileName}";
    }
}
