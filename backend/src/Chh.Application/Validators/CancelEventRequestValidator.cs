using Chh.Application.Dtos;
using Chh.Domain.Constants;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="CancelEventRequest"/> at the controller boundary (CHH-41/US-CHH-005-04 AC1).</summary>
public class CancelEventRequestValidator : AbstractValidator<CancelEventRequest>
{
    /// <summary>Configures the validation rules for <see cref="CancelEventRequest"/>.</summary>
    public CancelEventRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Please tell attendees why the event is cancelled.")
            .Must(r => r.Trim().Length is >= EventConstants.MinCancellationReasonLength and <= EventConstants.MaxCancellationReasonLength)
            .WithMessage($"Reason must be between {EventConstants.MinCancellationReasonLength} and {EventConstants.MaxCancellationReasonLength} characters.")
            .When(x => !string.IsNullOrEmpty(x.Reason), ApplyConditionTo.CurrentValidator);
    }
}
