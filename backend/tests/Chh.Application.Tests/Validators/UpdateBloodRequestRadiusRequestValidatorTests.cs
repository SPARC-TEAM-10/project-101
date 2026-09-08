using Chh.Application.Dtos;
using Chh.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Chh.Application.Tests.Validators;

public class UpdateBloodRequestRadiusRequestValidatorTests
{
    private readonly UpdateBloodRequestRadiusRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRadiusInRange_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(new UpdateBloodRequestRadiusRequest { SearchRadiusKm = 50 });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenRadiusBelowMinimum_HasValidationErrorForSearchRadiusKm()
    {
        var result = _validator.TestValidate(new UpdateBloodRequestRadiusRequest { SearchRadiusKm = 4 });

        result.ShouldHaveValidationErrorFor(x => x.SearchRadiusKm);
    }

    [Fact]
    public void Validate_WhenRadiusAboveMaximum_HasValidationErrorForSearchRadiusKm()
    {
        var result = _validator.TestValidate(new UpdateBloodRequestRadiusRequest { SearchRadiusKm = 101 });

        result.ShouldHaveValidationErrorFor(x => x.SearchRadiusKm);
    }
}
