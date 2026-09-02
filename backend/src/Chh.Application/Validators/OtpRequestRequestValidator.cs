using Chh.Application.Dtos;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="OtpRequestRequest"/> at the controller boundary.</summary>
public class OtpRequestRequestValidator : AbstractValidator<OtpRequestRequest>
{
    private const string MobileNumberPattern = "^[0-9]{10}$";
    private const string InvalidMobileNumberMessage = "Please enter a valid 10-digit mobile number";

    /// <summary>Configures the validation rules for <see cref="OtpRequestRequest"/>.</summary>
    public OtpRequestRequestValidator()
    {
        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage(InvalidMobileNumberMessage)
            .Matches(MobileNumberPattern).WithMessage(InvalidMobileNumberMessage);
    }
}
