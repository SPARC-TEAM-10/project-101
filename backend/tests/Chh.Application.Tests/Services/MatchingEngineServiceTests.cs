using Chh.Application.Contracts;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class MatchingEngineServiceTests
{
    private readonly Mock<IIndividualProfileRepository> _individualProfileRepository = new();
    private readonly MatchingEngineService _sut;

    private const decimal RequestLatitude = 9.9312m;
    private const decimal RequestLongitude = 76.2673m;

    public MatchingEngineServiceTests()
    {
        _sut = new MatchingEngineService(_individualProfileRepository.Object);
    }

    private static BloodRequest MakeRequest(BloodGroup bloodGroup, int searchRadiusKm, DateTimeOffset? expiresAtUtc = null) => new()
    {
        RequesterMobileNumber = "9876543210",
        PatientName = "Jane Doe",
        BloodGroup = bloodGroup,
        UnitsRequired = 1,
        LocationCityArea = "Kochi",
        Latitude = RequestLatitude,
        Longitude = RequestLongitude,
        SearchRadiusKm = searchRadiusKm,
        Urgency = UrgencyLevel.Emergency,
        Status = BloodRequestStatus.Matching,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        ExpiresAtUtc = expiresAtUtc ?? DateTimeOffset.UtcNow.AddHours(6)
    };

    private static IndividualProfile MakeDonor(
        BloodGroup bloodGroup,
        decimal? latitude,
        decimal? longitude,
        AccountStatus accountStatus = AccountStatus.Active,
        bool isReceiverOnly = false,
        DateTimeOffset? lastActiveAtUtc = null) => new()
    {
        MobileNumber = "9000000000",
        FullName = "Donor",
        Email = "donor@example.com",
        BloodGroup = bloodGroup,
        DateOfBirth = new DateOnly(1990, 1, 1),
        Gender = Gender.Male,
        LocationCityArea = "Kochi",
        Latitude = latitude,
        Longitude = longitude,
        AccountStatus = accountStatus,
        IsReceiverOnly = isReceiverOnly,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        LastActiveAtUtc = lastActiveAtUtc
    };

    private void SetupCandidates(params IndividualProfile[] candidates) =>
        _individualProfileRepository
            .Setup(r => r.GetActiveDonorsByBloodGroupsAsync(It.IsAny<IReadOnlySet<BloodGroup>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

    [Fact]
    public async Task FindEligibleDonorsAsync_ONegativeRequest_OnlyMatchesONegativeDonors()
    {
        IReadOnlySet<BloodGroup>? passedGroups = null;
        _individualProfileRepository
            .Setup(r => r.GetActiveDonorsByBloodGroupsAsync(It.IsAny<IReadOnlySet<BloodGroup>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlySet<BloodGroup>, CancellationToken>((groups, _) => passedGroups = groups)
            .ReturnsAsync([]);

        await _sut.FindEligibleDonorsAsync(MakeRequest(BloodGroup.ONegative, 20), CancellationToken.None);

        passedGroups.Should().BeEquivalentTo(new[] { BloodGroup.ONegative });
    }

    [Fact]
    public async Task FindEligibleDonorsAsync_ABPositiveRequest_MatchesAllEightGroups()
    {
        IReadOnlySet<BloodGroup>? passedGroups = null;
        _individualProfileRepository
            .Setup(r => r.GetActiveDonorsByBloodGroupsAsync(It.IsAny<IReadOnlySet<BloodGroup>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlySet<BloodGroup>, CancellationToken>((groups, _) => passedGroups = groups)
            .ReturnsAsync([]);

        await _sut.FindEligibleDonorsAsync(MakeRequest(BloodGroup.ABPositive, 20), CancellationToken.None);

        passedGroups.Should().HaveCount(8);
    }

    [Fact]
    public async Task FindEligibleDonorsAsync_DonorWithinRadius_IsIncluded()
    {
        // ~15km from the request's location — matches the Confluence AC2 example.
        SetupCandidates(MakeDonor(BloodGroup.OPositive, 10.0500m, 76.3300m));

        var result = await _sut.FindEligibleDonorsAsync(MakeRequest(BloodGroup.OPositive, 20), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].DistanceKm.Should().BeLessThanOrEqualTo(20m);
    }

    [Fact]
    public async Task FindEligibleDonorsAsync_DonorOutsideRadius_IsExcluded()
    {
        // ~25km away — matches the Confluence AC2 "excluded" example.
        SetupCandidates(MakeDonor(BloodGroup.OPositive, 10.1500m, 76.4000m));

        var result = await _sut.FindEligibleDonorsAsync(MakeRequest(BloodGroup.OPositive, 20), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindEligibleDonorsAsync_SuspendedDonor_IsExcluded()
    {
        SetupCandidates(MakeDonor(BloodGroup.OPositive, RequestLatitude, RequestLongitude, accountStatus: AccountStatus.Suspended));

        var result = await _sut.FindEligibleDonorsAsync(MakeRequest(BloodGroup.OPositive, 20), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindEligibleDonorsAsync_ReceiverOnlyDonor_IsExcluded()
    {
        SetupCandidates(MakeDonor(BloodGroup.OPositive, RequestLatitude, RequestLongitude, isReceiverOnly: true));

        var result = await _sut.FindEligibleDonorsAsync(MakeRequest(BloodGroup.OPositive, 20), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindEligibleDonorsAsync_DonorWithNullCoordinates_IsExcluded()
    {
        SetupCandidates(MakeDonor(BloodGroup.OPositive, null, null));

        var result = await _sut.FindEligibleDonorsAsync(MakeRequest(BloodGroup.OPositive, 100), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindEligibleDonorsAsync_NoCandidates_ReturnsEmptyListWithoutThrowing()
    {
        SetupCandidates();

        var result = await _sut.FindEligibleDonorsAsync(MakeRequest(BloodGroup.OPositive, 20), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindEligibleDonorsAsync_ResultIsSortedByAscendingDistance()
    {
        SetupCandidates(
            MakeDonor(BloodGroup.OPositive, 10.0500m, 76.3300m), // ~15km
            MakeDonor(BloodGroup.OPositive, 9.9400m, 76.2700m)); // very close

        var result = await _sut.FindEligibleDonorsAsync(MakeRequest(BloodGroup.OPositive, 50), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().BeInAscendingOrder(m => m.DistanceKm);
    }

    [Fact]
    public async Task FindEligibleDonorsAsync_CopiesLastActiveAtUtcOntoMatch()
    {
        var lastActive = DateTimeOffset.UtcNow.AddMinutes(-1);
        SetupCandidates(MakeDonor(BloodGroup.OPositive, RequestLatitude, RequestLongitude, lastActiveAtUtc: lastActive));

        var result = await _sut.FindEligibleDonorsAsync(MakeRequest(BloodGroup.OPositive, 20), CancellationToken.None);

        result.Should().ContainSingle().Which.LastActiveAtUtc.Should().Be(lastActive);
    }
}
