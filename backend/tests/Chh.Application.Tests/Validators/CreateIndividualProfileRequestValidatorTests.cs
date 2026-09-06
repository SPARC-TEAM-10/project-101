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

    [Fact]
    public void Validate_WhenAllFieldsAreValid_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenFullNameIsTooShort_HasValidationErrorForFullName()
    {
        var request = ValidRequest() with { FullName = "J" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_WhenEmailIsMalformed_HasValidationErrorForEmail()
    {
        var request = ValidRequest() with { Email = "not-an-email" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WhenUnderEighteen_HasValidationErrorForDateOfBirth()
    {
        var request = ValidRequest() with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-17)) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_WhenDateOfBirthIsInTheFuture_HasValidationErrorForDateOfBirth()
    {
        var request = ValidRequest() with { DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_WhenLocationIsEmpty_HasValidationErrorForLocation()
    {
        var request = ValidRequest() with { LocationCityArea = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.LocationCityArea);
    }

    [Fact]
    public void Validate_WhenOtherIllnessSelectedWithoutDetails_HasValidationErrorForOtherIllnessDetails()
    {
        var request = ValidRequest() with { IsOtherIllness = true, OtherIllnessDetails = null };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OtherIllnessDetails);
    }

    [Fact]
    public void Validate_WhenOtherIllnessSelectedWithDetails_HasNoValidationErrorForOtherIllnessDetails()
    {
        var request = ValidRequest() with { IsOtherIllness = true, OtherIllnessDetails = "Seasonal allergy" };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.OtherIllnessDetails);
    }

    [Fact]
    public void Validate_WhenOtherIllnessNotSelected_DoesNotRequireOtherIllnessDetails()
    {
        var request = ValidRequest() with { IsOtherIllness = false, OtherIllnessDetails = null };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.OtherIllnessDetails);
    }
}
