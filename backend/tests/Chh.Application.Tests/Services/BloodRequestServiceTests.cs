using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class BloodRequestServiceTests
{
    private readonly Mock<IBloodRequestRepository> _bloodRequestRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly BloodRequestService _sut;

    private const string RequesterMobileNumber = "9876543210";

    public BloodRequestServiceTests()
    {
        _sut = new BloodRequestService(_bloodRequestRepository.Object, _unitOfWork.Object);
    }

    private static CreateBloodRequestRequest ValidRequest() => new()
    {
        PatientName = "John Doe",
        BloodGroup = BloodGroup.OPositive,
        UnitsRequired = 2,
        LocationCityArea = "Kochi",
        Latitude = 9.9312m,
        Longitude = 76.2673m,
        SearchRadiusKm = 10,
        Urgency = UrgencyLevel.Emergency
    };

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_PersistsAndReturnsMatchingStatus()
    {
        var response = await _sut.CreateAsync(RequesterMobileNumber, ValidRequest(), CancellationToken.None);

        response.Status.Should().Be(BloodRequestStatus.Matching);
        response.PatientName.Should().Be("John Doe");
        _bloodRequestRepository.Verify(r => r.AddAsync(
            It.Is<BloodRequest>(b => b.RequesterMobileNumber == RequesterMobileNumber),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SetsExpiryToSixHoursAfterCreation()
    {
        var response = await _sut.CreateAsync(RequesterMobileNumber, ValidRequest(), CancellationToken.None);

        (response.ExpiresAtUtc - response.CreatedAtUtc).Should().Be(TimeSpan.FromHours(6));
    }

    [Fact]
    public async Task CreateAsync_DoesNotTrustClientForRequesterMobileNumber()
    {
        BloodRequest? captured = null;
        _bloodRequestRepository
            .Setup(r => r.AddAsync(It.IsAny<BloodRequest>(), It.IsAny<CancellationToken>()))
            .Callback<BloodRequest, CancellationToken>((b, _) => captured = b)
            .Returns(Task.CompletedTask);

        await _sut.CreateAsync(RequesterMobileNumber, ValidRequest(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.RequesterMobileNumber.Should().Be(RequesterMobileNumber);
    }
}
