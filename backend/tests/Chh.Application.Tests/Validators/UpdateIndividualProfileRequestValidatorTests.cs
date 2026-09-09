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

    [Fact]
    public void Validate_WhenBothCoordinatesOmitted_HasNoValidationError()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveValidationErrorFor(x => x.Latitude);
        result.ShouldNotHaveValidationErrorFor(x => x.Longitude);
    }

    [Fact]
    public void Validate_WhenBothCoordinatesProvided_HasNoValidationError()
    {
        var request = ValidRequest() with { Latitude = 9.9312m, Longitude = 76.2673m };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Latitude);
        result.ShouldNotHaveValidationErrorFor(x => x.Longitude);
    }

    [Fact]
    public void Validate_WhenLatitudeProvidedWithoutLongitude_HasValidationErrorForLongitude()
    {
        var request = ValidRequest() with { Latitude = 9.9312m };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }

    [Fact]
    public void Validate_WhenLongitudeProvidedWithoutLatitude_HasValidationErrorForLatitude()
    {
        var request = ValidRequest() with { Longitude = 76.2673m };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validate_WhenLatitudeOutOfRange_HasValidationErrorForLatitude(decimal latitude)
    {
        var request = ValidRequest() with { Latitude = latitude, Longitude = 76.2673m };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Validate_WhenLongitudeOutOfRange_HasValidationErrorForLongitude(decimal longitude)
    {
        var request = ValidRequest() with { Latitude = 9.9312m, Longitude = longitude };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }
}
