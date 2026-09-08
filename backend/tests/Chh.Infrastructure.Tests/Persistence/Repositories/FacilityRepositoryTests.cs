using Chh.Domain.Entities;
using Chh.Domain.Enums;
using Chh.Infrastructure.Persistence;
using Chh.Infrastructure.Persistence.Repositories;
using Chh.Infrastructure.Tests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chh.Infrastructure.Tests.Persistence.Repositories;

public class FacilityRepositoryTests
{
    private static ChhDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ChhDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ChhDbContext(options, new NoOpFieldEncryptor());
    }

    private static Facility CreateFacility(FacilityCategory category, FacilityVerificationStatus status) => new()
    {
        FacilityName = "Test Facility",
        Category = category,
        LicenseNumber = "LIC-TEST-001",
        Address = "1 Test Street",
        VerificationStatus = status,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow,
    };

    private static FacilityContact CreateContact(Guid facilityId, string mobile) => new()
    {
        FacilityId = facilityId,
        Name = "Test Contact",
        Designation = "Administrator",
        Mobile = mobile,
    };

    [Fact]
    public async Task GetByContactMobileNumberAsync_WhenContactMobileMatches_ReturnsOwningFacility()
    {
        await using var context = CreateContext();
        var facility = CreateFacility(FacilityCategory.Hospital, FacilityVerificationStatus.Verified);
        context.Facilities.Add(facility);
        context.FacilityContacts.Add(CreateContact(facility.Id, "9000000001"));
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var result = await repository.GetByContactMobileNumberAsync("9000000001", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(facility.Id);
        result.Category.Should().Be(FacilityCategory.Hospital);
    }

    [Fact]
    public async Task GetByContactMobileNumberAsync_WhenNoContactMatches_ReturnsNull()
    {
        await using var context = CreateContext();
        var facility = CreateFacility(FacilityCategory.Ngo, FacilityVerificationStatus.Verified);
        context.Facilities.Add(facility);
        context.FacilityContacts.Add(CreateContact(facility.Id, "9000000002"));
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var result = await repository.GetByContactMobileNumberAsync("9999999999", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByContactMobileNumberAsync_WhenFacilityHasMultipleContacts_MatchesOnAnyContact()
    {
        await using var context = CreateContext();
        var facility = CreateFacility(FacilityCategory.Hospital, FacilityVerificationStatus.Verified);
        context.Facilities.Add(facility);
        context.FacilityContacts.Add(CreateContact(facility.Id, "9000000001"));
        context.FacilityContacts.Add(CreateContact(facility.Id, "9000000003"));
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var result = await repository.GetByContactMobileNumberAsync("9000000003", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(facility.Id);
    }

    [Fact]
    public async Task GetByContactMobileNumberAsync_WhenFacilityIsPendingVerification_StillReturnsFacility()
    {
        await using var context = CreateContext();
        var facility = CreateFacility(FacilityCategory.Ngo, FacilityVerificationStatus.Pending);
        context.Facilities.Add(facility);
        context.FacilityContacts.Add(CreateContact(facility.Id, "9000000002"));
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var result = await repository.GetByContactMobileNumberAsync("9000000002", CancellationToken.None);

        result.Should().NotBeNull();
        result!.VerificationStatus.Should().Be(FacilityVerificationStatus.Pending);
    }
}
