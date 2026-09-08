using Chh.Application.Dtos;
using Chh.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Chh.Application.Tests.Validators;

public class UpdateIndividualProfileRequestValidatorTests
{
    private readonly UpdateIndividualProfileRequestValidator _validator = new();

    private static UpdateIndividualProfileRequest ValidRequest() => new()
    {
        LocationCityArea = "Kochi"
    };

    [Fact]
    public void Validate_WhenAllFieldsAreValid_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.IsValid.Should().BeTrue();
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

    [Fact]
    public void Validate_WhenOtherIllnessDetailsExceedsMaxLength_HasValidationErrorForOtherIllnessDetails()
    {
        var request = ValidRequest() with { IsOtherIllness = true, OtherIllnessDetails = new string('a', 201) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OtherIllnessDetails);
    }
}
