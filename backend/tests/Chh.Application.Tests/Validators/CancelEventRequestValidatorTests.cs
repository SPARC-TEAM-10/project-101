using Chh.Application.Dtos;
using Chh.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Chh.Application.Tests.Validators;

public class CancelEventRequestValidatorTests
{
    private readonly CancelEventRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenReasonIsValid_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(new CancelEventRequest { Reason = "The hall is unavailable after storm damage." });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenReasonIsEmpty_HasValidationErrorForReason()
    {
        var result = _validator.TestValidate(new CancelEventRequest { Reason = "" });

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_WhenReasonTooShort_HasValidationErrorForReason()
    {
        var result = _validator.TestValidate(new CancelEventRequest { Reason = "Rain." });

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_WhenReasonTooLong_HasValidationErrorForReason()
    {
        var result = _validator.TestValidate(new CancelEventRequest { Reason = new string('a', 301) });

        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
