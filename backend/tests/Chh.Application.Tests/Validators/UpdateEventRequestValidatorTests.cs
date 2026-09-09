using Chh.Application.Dtos;
using Chh.Application.Validators;
using Chh.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Chh.Application.Tests.Validators;

public class UpdateEventRequestValidatorTests
{
    private readonly UpdateEventRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenEveryFieldUnset_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(new UpdateEventRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenOnlyTitleIsValid_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(new UpdateEventRequest { Title = "New title for the camp" });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenTitleTooShort_HasValidationErrorForTitle()
    {
        var result = _validator.TestValidate(new UpdateEventRequest { Title = "Hi" });

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void Validate_WhenCapacityIsOutOfRange_HasValidationErrorForCapacity(int capacity)
    {
        var result = _validator.TestValidate(new UpdateEventRequest { Capacity = capacity });

        result.ShouldHaveValidationErrorFor(x => x.Capacity)
            .WithErrorMessage(Chh.Domain.Constants.EventConstants.CapacityRangeMessage);
    }

    [Fact]
    public void Validate_WhenLatitudeOutOfRange_HasValidationErrorForLatitude()
    {
        var result = _validator.TestValidate(new UpdateEventRequest { Latitude = 200m });

        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Fact]
    public void Validate_WhenCoordinatorContactIsNotTenDigits_HasValidationErrorForCoordinatorContact()
    {
        var result = _validator.TestValidate(new UpdateEventRequest { CoordinatorContact = "123" });

        result.ShouldHaveValidationErrorFor(x => x.CoordinatorContact);
    }

    [Fact]
    public void Validate_WhenEventTypeInvalid_HasValidationErrorForEventType()
    {
        var result = _validator.TestValidate(new UpdateEventRequest { EventType = (EventType)999 });

        result.ShouldHaveValidationErrorFor(x => x.EventType);
    }
}
