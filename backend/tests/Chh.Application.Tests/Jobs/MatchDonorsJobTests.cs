using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Jobs;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Jobs;

public class MatchDonorsJobTests
{
    private readonly Mock<IBloodRequestRepository> _bloodRequestRepository = new();
    private readonly Mock<IMatchingEngineService> _matchingEngineService = new();
    private readonly Mock<ILogger<MatchDonorsJob>> _logger = new();
    private readonly MatchDonorsJob _sut;

    public MatchDonorsJobTests()
    {
        _sut = new MatchDonorsJob(_bloodRequestRepository.Object, _matchingEngineService.Object, _logger.Object);
    }

    private static BloodRequest MakeRequest(DateTimeOffset expiresAtUtc) => new()
    {
        RequesterMobileNumber = "9876543210",
        PatientName = "Jane Doe",
        BloodGroup = BloodGroup.OPositive,
        UnitsRequired = 1,
        LocationCityArea = "Kochi",
        Latitude = 9.9312m,
        Longitude = 76.2673m,
        SearchRadiusKm = 20,
        Urgency = UrgencyLevel.Emergency,
        Status = BloodRequestStatus.Matching,
        CreatedAtUtc = DateTimeOffset.UtcNow.AddHours(-1),
        ExpiresAtUtc = expiresAtUtc
    };

    [Fact]
    public async Task RunAsync_ActiveRequest_CallsMatchingEngine()
    {
        var request = MakeRequest(DateTimeOffset.UtcNow.AddHours(5));
        _bloodRequestRepository.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _matchingEngineService
            .Setup(m => m.FindEligibleDonorsAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.RunAsync(request.Id, CancellationToken.None);

        _matchingEngineService.Verify(m => m.FindEligibleDonorsAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_RequestNotFound_DoesNotCallMatchingEngine()
    {
        var requestId = Guid.NewGuid();
        _bloodRequestRepository.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync((BloodRequest?)null);

        var act = () => _sut.RunAsync(requestId, CancellationToken.None);

        await act.Should().NotThrowAsync();
        _matchingEngineService.Verify(m => m.FindEligibleDonorsAsync(It.IsAny<BloodRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_ExpiredRequest_DoesNotCallMatchingEngine()
    {
        var request = MakeRequest(DateTimeOffset.UtcNow.AddHours(-1));
        _bloodRequestRepository.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>())).ReturnsAsync(request);

        await _sut.RunAsync(request.Id, CancellationToken.None);

        _matchingEngineService.Verify(m => m.FindEligibleDonorsAsync(It.IsAny<BloodRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
