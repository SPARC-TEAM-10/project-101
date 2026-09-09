using Chh.Application.Dtos;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using Chh.Infrastructure.Persistence;
using Chh.Infrastructure.Persistence.Repositories;
using Chh.Infrastructure.Tests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chh.Infrastructure.Tests.Persistence.Repositories;

/// <summary>Tests for <see cref="FacilityRepository.SearchAsync"/> and <see cref="FacilityRepository.GetVerifiedByIdAsync"/> (CHH-82/Epic CHH-68).</summary>
public class FacilityRepositorySearchTests
{
    private static ChhDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ChhDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ChhDbContext(options, new NoOpFieldEncryptor());
    }

    private static Facility CreateFacility(
        string name, string address, FacilityCategory category, FacilityVerificationStatus status) => new()
    {
        FacilityName = name,
        Category = category,
        SubCategory = FacilitySubCategory.Government,
        LicenseNumber = $"LIC-{Guid.NewGuid():N}",
        Address = address,
        VerificationStatus = status,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    private static SearchFacilitiesRequest MakeRequest(
        string? q = null, FacilityCategory? category = null, int page = 1, int pageSize = 20) => new()
    {
        Q = q,
        Category = category,
        Page = page,
        PageSize = pageSize
    };

    [Fact]
    public async Task SearchAsync_OnlyReturnsVerifiedFacilities_EvenWhenPendingAndRejectedExist()
    {
        await using var context = CreateContext();
        context.Facilities.AddRange(
            CreateFacility("Verified Hospital", "1 Main St", FacilityCategory.Hospital, FacilityVerificationStatus.Verified),
            CreateFacility("Pending Hospital", "2 Main St", FacilityCategory.Hospital, FacilityVerificationStatus.Pending),
            CreateFacility("Rejected Hospital", "3 Main St", FacilityCategory.Hospital, FacilityVerificationStatus.Rejected));
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var results = await repository.SearchAsync(MakeRequest(), CancellationToken.None);

        results.Should().ContainSingle(f => f.FacilityName == "Verified Hospital");
    }

    [Fact]
    public async Task SearchAsync_QMatchesFacilityNameCaseInsensitive()
    {
        await using var context = CreateContext();
        context.Facilities.Add(
            CreateFacility("City General Hospital", "1 Main St", FacilityCategory.Hospital, FacilityVerificationStatus.Verified));
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var results = await repository.SearchAsync(MakeRequest(q: "GENERAL"), CancellationToken.None);

        results.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchAsync_QMatchesAddressCaseInsensitive()
    {
        await using var context = CreateContext();
        context.Facilities.Add(
            CreateFacility("Unrelated Name", "42 Kochi Road", FacilityCategory.Hospital, FacilityVerificationStatus.Verified));
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var results = await repository.SearchAsync(MakeRequest(q: "KOCHI"), CancellationToken.None);

        results.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchAsync_QWithNoMatch_ReturnsEmpty()
    {
        await using var context = CreateContext();
        context.Facilities.Add(
            CreateFacility("City General Hospital", "1 Main St", FacilityCategory.Hospital, FacilityVerificationStatus.Verified));
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var results = await repository.SearchAsync(MakeRequest(q: "Nonexistent"), CancellationToken.None);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_CategoryFilterNarrowsToAmbulanceOnly()
    {
        await using var context = CreateContext();
        context.Facilities.AddRange(
            CreateFacility("Hospital One", "1 Main St", FacilityCategory.Hospital, FacilityVerificationStatus.Verified),
            CreateFacility("Ambulance One", "2 Main St", FacilityCategory.Ambulance, FacilityVerificationStatus.Verified));
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var results = await repository.SearchAsync(MakeRequest(category: FacilityCategory.Ambulance), CancellationToken.None);

        results.Should().ContainSingle(f => f.FacilityName == "Ambulance One");
    }

    [Fact]
    public async Task GetVerifiedByIdAsync_WhenFacilityIsVerified_ReturnsFacilityWithContacts()
    {
        await using var context = CreateContext();
        var facility = CreateFacility("City General Hospital", "1 Main St", FacilityCategory.Hospital, FacilityVerificationStatus.Verified);
        context.Facilities.Add(facility);
        context.FacilityContacts.Add(new FacilityContact { FacilityId = facility.Id, Name = "Jane Doe", Designation = "Administrator", Mobile = "9876543210" });
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var result = await repository.GetVerifiedByIdAsync(facility.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Contacts.Should().ContainSingle(c => c.Mobile == "9876543210");
    }

    [Theory]
    [InlineData(FacilityVerificationStatus.Pending)]
    [InlineData(FacilityVerificationStatus.Rejected)]
    public async Task GetVerifiedByIdAsync_WhenFacilityIsNotVerified_ReturnsNull(FacilityVerificationStatus status)
    {
        await using var context = CreateContext();
        var facility = CreateFacility("Unverified Hospital", "1 Main St", FacilityCategory.Hospital, status);
        context.Facilities.Add(facility);
        await context.SaveChangesAsync();
        var repository = new FacilityRepository(context);

        var result = await repository.GetVerifiedByIdAsync(facility.Id, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetVerifiedByIdAsync_WhenFacilityDoesNotExist_ReturnsNull()
    {
        await using var context = CreateContext();
        var repository = new FacilityRepository(context);

        var result = await repository.GetVerifiedByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }
}
