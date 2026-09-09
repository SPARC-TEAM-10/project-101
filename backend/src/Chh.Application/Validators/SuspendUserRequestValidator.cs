using Chh.Application.Dtos;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="SuspendUserRequest"/> at the controller boundary (CHH-76 AC1).</summary>
public class SuspendUserRequestValidator : AbstractValidator<SuspendUserRequest>
{
    private const int MinReasonLength = 3;
    private const int MaxReasonLength = 500;

    /// <summary>Configures the validation rules for <see cref="SuspendUserRequest"/>.</summary>
    public SuspendUserRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Enter a reason for suspending this account.")
            .MinimumLength(MinReasonLength).WithMessage($"Reason must be at least {MinReasonLength} characters.")
            .MaximumLength(MaxReasonLength);
    }
}
