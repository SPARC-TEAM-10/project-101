using Chh.Application.Abstractions;
using Chh.Application.Contracts;
using Chh.Application.Dtos;
using Chh.Application.Services;
using Chh.Domain.Entities;
using Chh.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace Chh.Application.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<IFacilityRepository> _facilityRepository = new();
    private readonly Mock<IEventRepository> _eventRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly EventService _sut;

    public EventServiceTests()
    {
        _sut = new EventService(_facilityRepository.Object, _eventRepository.Object, _unitOfWork.Object);
    }

    private static readonly DateTimeOffset Start = DateTimeOffset.UtcNow.AddDays(2);

    private static CreateEventRequest ValidRequest() => new()
    {
        Title = "Community blood drive — Kaloor",
        EventType = EventType.BloodDonationCamp,
        Description = "Walk-in donors welcome. Bring a photo ID. Refreshments provided.",
        VenueName = "Kaloor Community Hall",
        VenueAddress = "Stadium Link Road, Kaloor, Kochi 682017",
        Latitude = 9.996m,
        Longitude = 76.299m,
        StartAtUtc = Start,
        EndAtUtc = Start.AddHours(5),
        Capacity = 60,
        CoordinatorName = "Dr Anitha Varghese",
        CoordinatorContact = "9000010023"
    };

    private static Facility VerifiedFacility() => new()
    {
        FacilityName = "Kochi Metro Hospital",
        Category = FacilityCategory.Hospital,
        SubCategory = FacilitySubCategory.Government,
        LicenseNumber = "KL-HOSP-448120",
        Address = "Marine Drive, Kochi",
        VerificationStatus = FacilityVerificationStatus.Verified,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task CreateAsync_WhenFacilityIsVerified_PersistsAndReturnsDto()
    {
        var facility = VerifiedFacility();
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync("9876543210", It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var result = await _sut.CreateAsync("9876543210", ValidRequest(), CancellationToken.None);

        result.Title.Should().Be("Community blood drive — Kaloor");
        result.FacilityId.Should().Be(facility.Id);
        result.Status.Should().Be(EventStatus.Published);
        _eventRepository.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenFacilityIsPending_ThrowsFacilityNotVerifiedException()
    {
        var facility = VerifiedFacility();
        facility.VerificationStatus = FacilityVerificationStatus.Pending;
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync("9876543210", It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var act = () => _sut.CreateAsync("9876543210", ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<FacilityNotVerifiedException>();
        _eventRepository.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCallerHasNoFacility_ThrowsFacilityNotVerifiedException()
    {
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync("9876543210", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Facility?)null);

        var act = () => _sut.CreateAsync("9876543210", ValidRequest(), CancellationToken.None);

        await act.Should().ThrowAsync<FacilityNotVerifiedException>();
        _eventRepository.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
