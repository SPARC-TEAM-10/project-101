using Chh.Application.Dtos;
using Chh.Domain.Constants;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="OtpRequestRequest"/> at the controller boundary.</summary>
public class OtpRequestRequestValidator : AbstractValidator<OtpRequestRequest>
{
    /// <summary>Configures the validation rules for <see cref="OtpRequestRequest"/>.</summary>
    public OtpRequestRequestValidator()
    {
        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage(OtpConstants.InvalidMobileNumberMessage)
            .Matches(OtpConstants.MobileNumberPattern).WithMessage(OtpConstants.InvalidMobileNumberMessage);
    }
}
