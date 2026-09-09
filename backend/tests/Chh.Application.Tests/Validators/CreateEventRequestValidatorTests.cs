using Chh.Application.Dtos;
using Chh.Application.Validators;
using Chh.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Chh.Application.Tests.Validators;

public class CreateEventRequestValidatorTests
{
    private readonly CreateEventRequestValidator _validator = new();

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

    [Fact]
    public void Validate_WhenAllFieldsAreValid_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenTitleTooShort_HasValidationErrorForTitle()
    {
        var request = ValidRequest() with { Title = "Camp" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WhenStartIsInThePast_HasValidationErrorForStartAtUtc()
    {
        var request = ValidRequest() with { StartAtUtc = DateTimeOffset.UtcNow.AddHours(-1) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.StartAtUtc)
            .WithErrorMessage(Chh.Domain.Constants.EventConstants.StartMustBeFutureMessage);
    }

    [Fact]
    public void Validate_WhenStartIsLessThanOneHourAway_HasValidationErrorForStartAtUtc()
    {
        var request = ValidRequest() with { StartAtUtc = DateTimeOffset.UtcNow.AddMinutes(30) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.StartAtUtc);
    }

    [Fact]
    public void Validate_WhenEndIsBeforeStart_HasValidationErrorForEndAtUtc()
    {
        var request = ValidRequest() with { EndAtUtc = Start.AddHours(-1) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.EndAtUtc)
            .WithErrorMessage(Chh.Domain.Constants.EventConstants.EndMustBeAfterStartMessage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Validate_WhenCapacityIsOutOfRange_HasValidationErrorForCapacity(int capacity)
    {
        var request = ValidRequest() with { Capacity = capacity };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Capacity)
            .WithErrorMessage(Chh.Domain.Constants.EventConstants.CapacityRangeMessage);
    }

    [Fact]
    public void Validate_WhenRsvpCutoffIsAfterStart_HasValidationErrorForRsvpCutoffAtUtc()
    {
        var request = ValidRequest() with { RsvpCutoffAtUtc = Start.AddHours(1) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.RsvpCutoffAtUtc)
            .WithErrorMessage(Chh.Domain.Constants.EventConstants.RsvpCutoffMustPrecedeStartMessage);
    }

    [Fact]
    public void Validate_WhenRsvpCutoffIsBeforeStart_HasNoValidationErrorForRsvpCutoffAtUtc()
    {
        var request = ValidRequest() with { RsvpCutoffAtUtc = Start.AddHours(-1) };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.RsvpCutoffAtUtc);
    }

    [Fact]
    public void Validate_WhenCoordinatorContactIsNotTenDigits_HasValidationErrorForCoordinatorContact()
    {
        var request = ValidRequest() with { CoordinatorContact = "12345" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.CoordinatorContact);
    }
}
