using Chh.Application.Dtos;
using Chh.Application.Validators;
using Chh.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Chh.Application.Tests.Validators;

public class CreateIndividualProfileRequestValidatorTests
{
    private readonly CreateIndividualProfileRequestValidator _validator = new();

    private static CreateIndividualProfileRequest ValidRequest() => new()
    {
        MobileNumber = "9876543210",
        FullName = "Jane Doe",
        Email = "jane@example.com",
        BloodGroup = BloodGroup.OPositive,
        DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
        Gender = Gender.Female,
        LocationCityArea = "Kochi"
    };

    [Fact(DisplayName = "TC-CHH-F02-07: Validate_WhenAllFieldsAreValid_HasNoValidationErrors")]
    public void Validate_WhenAllFieldsAreValid_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "TC-CHH-F02-08: Validate_WhenFullNameIsTooShort_HasValidationErrorForFullName")]
    public void Validate_WhenFullNameIsTooShort_HasValidationErrorForFullName()
    {
        var request = ValidRequest() with { FullName = "J" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact(DisplayName = "TC-CHH-F02-09: Validate_WhenEmailIsMalformed_HasValidationErrorForEmail")]
    public void Validate_WhenEmailIsMalformed_HasValidationErrorForEmail()
    {
        var request = ValidRequest() with { Email = "not-an-email" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact(DisplayName = "TC-CHH-F02-10: Validate_WhenUnderEighteen_HasValidationErrorForDateOfBirth")]
    public void Validate_WhenUnderEighteen_HasValidationErrorForDateOfBirth()
    {
        var request = ValidRequest() with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-17)) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact(DisplayName = "TC-CHH-F02-11: Validate_WhenDateOfBirthIsInTheFuture_HasValidationErrorForDateOfBirth")]
    public void Validate_WhenDateOfBirthIsInTheFuture_HasValidationErrorForDateOfBirth()
    {
        var request = ValidRequest() with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact(DisplayName = "TC-CHH-F02-12: Validate_WhenLocationIsEmpty_HasValidationErrorForLocation")]
    public void Validate_WhenLocationIsEmpty_HasValidationErrorForLocation()
    {
        var request = ValidRequest() with { LocationCityArea = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.LocationCityArea);
    }

    [Fact(DisplayName = "TC-CHH-F02-13: Validate_WhenOtherIllnessSelectedWithoutDetails_HasValidationErrorForOtherIllnessDetails")]
    public void Validate_WhenOtherIllnessSelectedWithoutDetails_HasValidationErrorForOtherIllnessDetails()
    {
        var request = ValidRequest() with { IsOtherIllness = true, OtherIllnessDetails = null };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OtherIllnessDetails);
    }

    [Fact(DisplayName = "TC-CHH-F02-14: Validate_WhenOtherIllnessSelectedWithDetails_HasNoValidationErrorForOtherIllnessDetails")]
    public void Validate_WhenOtherIllnessSelectedWithDetails_HasNoValidationErrorForOtherIllnessDetails()
    {
        var request = ValidRequest() with { IsOtherIllness = true, OtherIllnessDetails = "Seasonal allergy" };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.OtherIllnessDetails);
    }

    [Fact(DisplayName = "TC-CHH-F02-15: Validate_WhenOtherIllnessNotSelected_DoesNotRequireOtherIllnessDetails")]
    public void Validate_WhenOtherIllnessNotSelected_DoesNotRequireOtherIllnessDetails()
    {
        var request = ValidRequest() with { IsOtherIllness = false, OtherIllnessDetails = null };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.OtherIllnessDetails);
    }
}
