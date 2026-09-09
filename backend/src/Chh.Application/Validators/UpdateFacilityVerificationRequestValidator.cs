using Chh.Application.Dtos;
using Chh.Domain.Enums;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="UpdateFacilityVerificationRequest"/> at the controller boundary (CHH-75).</summary>
public class UpdateFacilityVerificationRequestValidator : AbstractValidator<UpdateFacilityVerificationRequest>
{
    private const int MaxRejectionReasonLength = 500;

    /// <summary>Configures the validation rules for <see cref="UpdateFacilityVerificationRequest"/>.</summary>
    public UpdateFacilityVerificationRequestValidator()
    {
        RuleFor(x => x.Decision)
            .IsInEnum().WithMessage("Select Approve or Reject.");

        // AC2: rejecting without a reason is invalid — approving with one is simply ignored by the
        // service, not rejected here, since the frontend never sends both.
        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Enter a reason for rejecting this facility.")
            .MaximumLength(MaxRejectionReasonLength)
            .When(x => x.Decision == FacilityVerificationDecision.Reject);
    }
}
