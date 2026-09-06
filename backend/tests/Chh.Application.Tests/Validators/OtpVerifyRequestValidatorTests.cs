using Chh.Application.Dtos;
using Chh.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Chh.Application.Tests.Validators;

public class OtpVerifyRequestValidatorTests
{
    private readonly OtpVerifyRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenMobileNumberAndOtpCodeAreValid_HasNoValidationErrors()
    {
        var request = new OtpVerifyRequest { MobileNumber = "9876543210", OtpCode = "123456" };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("98765432")]
    [InlineData("987654321a")]
    [InlineData("98765432100")]
    public void Validate_WhenMobileNumberIsMalformed_HasValidationErrorForMobileNumber(string mobileNumber)
    {
        var request = new OtpVerifyRequest { MobileNumber = mobileNumber, OtpCode = "123456" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.MobileNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("12345a")]
    [InlineData("1234567")]
    public void Validate_WhenOtpCodeIsMalformed_HasValidationErrorForOtpCode(string otpCode)
    {
        var request = new OtpVerifyRequest { MobileNumber = "9876543210", OtpCode = otpCode };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OtpCode);
    }
}
