using Chh.Application.Dtos;
using Chh.Domain.Constants;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="OtpVerifyRequest"/> at the controller boundary.</summary>
public class OtpVerifyRequestValidator : AbstractValidator<OtpVerifyRequest>
{
    /// <summary>Configures the validation rules for <see cref="OtpVerifyRequest"/>.</summary>
    public OtpVerifyRequestValidator()
    {
        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage(OtpConstants.InvalidMobileNumberMessage)
            .Matches(OtpConstants.MobileNumberPattern).WithMessage(OtpConstants.InvalidMobileNumberMessage);

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage(OtpConstants.InvalidOtpCodeShapeMessage)
            .Matches(OtpConstants.OtpCodePattern).WithMessage(OtpConstants.InvalidOtpCodeShapeMessage);
    }
}
