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

public class EventAttendanceServiceTests
{
    private readonly Mock<IEventRepository> _eventRepository = new();
    private readonly Mock<IEventRsvpRepository> _eventRsvpRepository = new();
    private readonly Mock<IFacilityRepository> _facilityRepository = new();
    private readonly Mock<IIndividualProfileRepository> _individualProfileRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly EventAttendanceService _sut;

    public EventAttendanceServiceTests()
    {
        _sut = new EventAttendanceService(
            _eventRepository.Object,
            _eventRsvpRepository.Object,
            _facilityRepository.Object,
            _individualProfileRepository.Object,
            _unitOfWork.Object);
    }

    private const string OwnerMobileNumber = "9876543210";
    private const string OtherFacilityMobileNumber = "9000000002";

    private static readonly DateTimeOffset Start = DateTimeOffset.UtcNow.AddHours(3);

    private static Event MakeEvent(Guid facilityId, EventStatus status = EventStatus.Published) => new()
    {
        FacilityId = facilityId,
        Title = "Community blood drive — Kaloor",
        EventType = EventType.BloodDonationCamp,
        Description = "Walk-in donors welcome.",
        VenueName = "Kaloor Community Hall",
        VenueAddress = "Stadium Link Road, Kaloor, Kochi 682017",
        Latitude = 9.996m,
        Longitude = 76.299m,
        StartAtUtc = Start,
        EndAtUtc = Start.AddHours(5),
        Capacity = 60,
        CoordinatorName = "Dr Anitha Varghese",
        CoordinatorContact = "9000010023",
        Status = status,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    private static Facility MakeFacility(string contactMobile, string contactName = "A. Thomas") => new()
    {
        FacilityName = "Kochi Metro Hospital",
        Category = FacilityCategory.Hospital,
        SubCategory = FacilitySubCategory.Government,
        LicenseNumber = "KL-HOSP-448120",
        Address = "Marine Drive, Kochi",
        VerificationStatus = FacilityVerificationStatus.Verified,
        CreatedAtUtc = DateTimeOffset.UtcNow,
        UpdatedAtUtc = DateTimeOffset.UtcNow,
        Contacts = new List<FacilityContact>
        {
            new() { Name = contactName, Designation = "Blood bank officer", Mobile = contactMobile, SortOrder = 1 }
        }
    };

    private static EventRsvp MakeRsvp(Guid eventId, EventRsvpStatus status = EventRsvpStatus.Going) => new()
    {
        EventId = eventId,
        IndividualProfileId = Guid.NewGuid(),
        ReferenceCode = "A1",
        Status = status,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    private static IndividualProfile MakeProfile(string mobileNumber) => new()
    {
        MobileNumber = mobileNumber,
        FullName = "Nithya Menon",
        Email = "nithya@example.com",
        BloodGroup = BloodGroup.OPositive,
        DateOfBirth = new DateOnly(1990, 1, 1),
        Gender = Gender.Female,
        LocationCityArea = "Kochi",
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    // --- SearchParticipantsAsync ---

    [Fact]
    public async Task SearchParticipantsAsync_ReturnsNull_WhenEventDoesNotExist()
    {
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventWithFacilityNameResult?)null);

        var result = await _sut.SearchParticipantsAsync(OwnerMobileNumber, Guid.NewGuid(), "nith", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchParticipantsAsync_WhenCallerDoesNotOwnEvent_ThrowsEventNotOwnedByCallerException()
    {
        var facility = MakeFacility(OwnerMobileNumber);
        var evt = MakeEvent(Guid.NewGuid());
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OtherFacilityMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var act = () => _sut.SearchParticipantsAsync(OtherFacilityMobileNumber, evt.Id, "nith", CancellationToken.None);

        await act.Should().ThrowAsync<EventNotOwnedByCallerException>();
    }

    [Theory]
    [InlineData("ni")]
    [InlineData("")]
    public async Task SearchParticipantsAsync_WhenSearchTooShortAndNotAMobileNumber_ThrowsChhValidationException(string search)
    {
        var facility = MakeFacility(OwnerMobileNumber);
        var evt = MakeEvent(facility.Id);
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);

        var act = () => _sut.SearchParticipantsAsync(OwnerMobileNumber, evt.Id, search, CancellationToken.None);

        await act.Should().ThrowAsync<ChhValidationException>();
    }

    [Fact]
    public async Task SearchParticipantsAsync_WhenSearchIsAFullMobileNumber_Succeeds()
    {
        var facility = MakeFacility(OwnerMobileNumber);
        var evt = MakeEvent(facility.Id);
        var rsvp = MakeRsvp(evt.Id);
        _eventRepository
            .Setup(r => r.GetByIdWithFacilityNameAsync(evt.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventWithFacilityNameResult { Event = evt, FacilityName = "Kochi Metro Hospital" });
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository
            .Setup(r => r.SearchParticipantsAsync(evt.Id, "9000000001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new EventRsvpWithProfileResult { EventRsvp = rsvp, FullName = "Nithya Menon", MobileNumber = "9000000001" } });

        var result = await _sut.SearchParticipantsAsync(OwnerMobileNumber, evt.Id, "9000000001", CancellationToken.None);

        result.Should().ContainSingle();
        result!.Single().MaskedMobileNumber.Should().Be("+91 ••••••0001");
    }

    // --- MarkAttendedAsync ---

    [Fact]
    public async Task MarkAttendedAsync_Success_SetsAttendedStatusAndMarkerName()
    {
        var facility = MakeFacility(OwnerMobileNumber, "A. Thomas");
        var evt = MakeEvent(facility.Id);
        // Within the check-in window: 1 hour before start until the event ends.
        evt.StartAtUtc = DateTimeOffset.UtcNow.AddMinutes(30);
        evt.EndAtUtc = evt.StartAtUtc.AddHours(5);
        var rsvp = MakeRsvp(evt.Id);
        var profile = MakeProfile("9000000001");
        _eventRepository.Setup(r => r.GetTrackedByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository.Setup(r => r.GetTrackedByIdAsync(rsvp.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rsvp);
        _individualProfileRepository
            .Setup(r => r.GetTrackedByIdAsync(rsvp.IndividualProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _sut.MarkAttendedAsync(OwnerMobileNumber, evt.Id, rsvp.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be(EventRsvpStatus.Attended);
        result.AttendedByName.Should().Be("A. Thomas");
        rsvp.Status.Should().Be(EventRsvpStatus.Attended);
        rsvp.AttendedAtUtc.Should().NotBeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAttendedAsync_WhenAlreadyAttended_ThrowsAlreadyAttendedException()
    {
        var facility = MakeFacility(OwnerMobileNumber);
        var evt = MakeEvent(facility.Id);
        var rsvp = MakeRsvp(evt.Id, EventRsvpStatus.Attended);
        _eventRepository.Setup(r => r.GetTrackedByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository.Setup(r => r.GetTrackedByIdAsync(rsvp.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rsvp);

        var act = () => _sut.MarkAttendedAsync(OwnerMobileNumber, evt.Id, rsvp.Id, CancellationToken.None);

        await act.Should().ThrowAsync<AlreadyAttendedException>();
    }

    [Fact]
    public async Task MarkAttendedAsync_WhenRsvpWasCancelled_ThrowsRsvpNotEligibleForAttendanceException()
    {
        var facility = MakeFacility(OwnerMobileNumber);
        var evt = MakeEvent(facility.Id);
        var rsvp = MakeRsvp(evt.Id, EventRsvpStatus.Cancelled);
        _eventRepository.Setup(r => r.GetTrackedByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository.Setup(r => r.GetTrackedByIdAsync(rsvp.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rsvp);

        var act = () => _sut.MarkAttendedAsync(OwnerMobileNumber, evt.Id, rsvp.Id, CancellationToken.None);

        await act.Should().ThrowAsync<RsvpNotEligibleForAttendanceException>();
    }

    [Fact]
    public async Task MarkAttendedAsync_BeforeCheckInWindowOpens_ThrowsAttendanceOutsideWindowException()
    {
        var facility = MakeFacility(OwnerMobileNumber);
        // Starts in 3 hours — the check-in window (1 hour before start) hasn't opened yet.
        var evt = MakeEvent(facility.Id);
        var rsvp = MakeRsvp(evt.Id);
        _eventRepository.Setup(r => r.GetTrackedByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository.Setup(r => r.GetTrackedByIdAsync(rsvp.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rsvp);

        var act = () => _sut.MarkAttendedAsync(OwnerMobileNumber, evt.Id, rsvp.Id, CancellationToken.None);

        await act.Should().ThrowAsync<AttendanceOutsideWindowException>();
    }

    [Fact]
    public async Task MarkAttendedAsync_AfterEventEnds_ThrowsAttendanceOutsideWindowException()
    {
        var facility = MakeFacility(OwnerMobileNumber);
        var evt = MakeEvent(facility.Id);
        evt.StartAtUtc = DateTimeOffset.UtcNow.AddHours(-6);
        evt.EndAtUtc = DateTimeOffset.UtcNow.AddHours(-1);
        var rsvp = MakeRsvp(evt.Id);
        _eventRepository.Setup(r => r.GetTrackedByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository.Setup(r => r.GetTrackedByIdAsync(rsvp.Id, It.IsAny<CancellationToken>())).ReturnsAsync(rsvp);

        var act = () => _sut.MarkAttendedAsync(OwnerMobileNumber, evt.Id, rsvp.Id, CancellationToken.None);

        await act.Should().ThrowAsync<AttendanceOutsideWindowException>();
    }

    [Fact]
    public async Task MarkAttendedAsync_WhenCallerDoesNotOwnEvent_ThrowsEventNotOwnedByCallerException()
    {
        var callerFacility = MakeFacility(OtherFacilityMobileNumber);
        var evt = MakeEvent(Guid.NewGuid());
        var rsvp = MakeRsvp(evt.Id);
        _eventRepository.Setup(r => r.GetTrackedByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OtherFacilityMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(callerFacility);

        var act = () => _sut.MarkAttendedAsync(OtherFacilityMobileNumber, evt.Id, rsvp.Id, CancellationToken.None);

        await act.Should().ThrowAsync<EventNotOwnedByCallerException>();
        _eventRsvpRepository.Verify(r => r.GetTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAttendedAsync_ReturnsNull_WhenEventDoesNotExist()
    {
        _eventRepository
            .Setup(r => r.GetTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var result = await _sut.MarkAttendedAsync(OwnerMobileNumber, Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task MarkAttendedAsync_ReturnsNull_WhenRsvpDoesNotExist()
    {
        var facility = MakeFacility(OwnerMobileNumber);
        var evt = MakeEvent(facility.Id);
        _eventRepository.Setup(r => r.GetTrackedByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _facilityRepository
            .Setup(r => r.GetByContactMobileNumberAsync(OwnerMobileNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(facility);
        _eventRsvpRepository
            .Setup(r => r.GetTrackedByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventRsvp?)null);

        var result = await _sut.MarkAttendedAsync(OwnerMobileNumber, evt.Id, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }
}
