using Chh.Application.Dtos;
using Chh.Application.Validators;
using Chh.Domain.Constants;
using Chh.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Chh.Application.Tests.Validators;

public class CreateBloodRequestRequestValidatorTests
{
    private readonly CreateBloodRequestRequestValidator _validator = new();

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
    public void Validate_WhenAllFieldsAreValid_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRadiusIsBelowMinimum_HasRadiusTooSmallError()
    {
        var request = ValidRequest() with { SearchRadiusKm = 4 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SearchRadiusKm)
            .WithErrorMessage(BloodRequestConstants.RadiusTooSmallMessage);
    }

    [Fact]
    public void Validate_WhenRadiusIsAtMinimum_HasNoValidationErrorForRadius()
    {
        var request = ValidRequest() with { SearchRadiusKm = BloodRequestConstants.MinSearchRadiusKm };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.SearchRadiusKm);
    }

    [Fact]
    public void Validate_WhenRadiusIsAboveMaximum_HasRadiusTooLargeError()
    {
        var request = ValidRequest() with { SearchRadiusKm = 101 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SearchRadiusKm)
            .WithErrorMessage(BloodRequestConstants.RadiusTooLargeMessage);
    }

    [Fact]
    public void Validate_WhenRadiusIsAtMaximum_HasNoValidationErrorForRadius()
    {
        var request = ValidRequest() with { SearchRadiusKm = BloodRequestConstants.MaxSearchRadiusKm };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.SearchRadiusKm);
    }

    [Fact]
    public void Validate_WhenUnitsRequiredIsZero_HasValidationErrorForUnitsRequired()
    {
        var request = ValidRequest() with { UnitsRequired = 0 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UnitsRequired);
    }

    [Fact]
    public void Validate_WhenPatientNameIsEmpty_HasValidationErrorForPatientName()
    {
        var request = ValidRequest() with { PatientName = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PatientName);
    }

    [Fact]
    public void Validate_WhenLocationCityAreaIsEmpty_HasValidationErrorForLocationCityArea()
    {
        var request = ValidRequest() with { LocationCityArea = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.LocationCityArea);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validate_WhenLatitudeIsOutOfRange_HasLocationNotResolvableError(decimal latitude)
    {
        var request = ValidRequest() with { Latitude = latitude };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Latitude)
            .WithErrorMessage(BloodRequestConstants.LocationNotResolvableMessage);
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Validate_WhenLongitudeIsOutOfRange_HasLocationNotResolvableError(decimal longitude)
    {
        var request = ValidRequest() with { Longitude = longitude };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Longitude)
            .WithErrorMessage(BloodRequestConstants.LocationNotResolvableMessage);
    }
}
