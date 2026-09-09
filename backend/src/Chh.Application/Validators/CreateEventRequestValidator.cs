using Chh.Application.Dtos;
using Chh.Domain.Constants;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="CreateEventRequest"/> at the controller boundary (CHH-38/US-CHH-005-01, spec §6.2).</summary>
public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    private static readonly System.Text.RegularExpressions.Regex MobilePattern =
        new(@"^\d{10}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>Configures the validation rules for <see cref="CreateEventRequest"/>.</summary>
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Please enter an event title.")
            .Must(t => t.Trim().Length is >= EventConstants.MinTitleLength and <= EventConstants.MaxTitleLength)
            .WithMessage($"Title must be between {EventConstants.MinTitleLength} and {EventConstants.MaxTitleLength} characters.")
            .When(x => !string.IsNullOrEmpty(x.Title), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.EventType)
            .IsInEnum().WithMessage("Please select an event type.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Please enter a description.")
            .Must(d => d.Trim().Length is >= EventConstants.MinDescriptionLength and <= EventConstants.MaxDescriptionLength)
            .WithMessage($"Description must be between {EventConstants.MinDescriptionLength} and {EventConstants.MaxDescriptionLength} characters.")
            .When(x => !string.IsNullOrEmpty(x.Description), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.VenueName)
            .NotEmpty().WithMessage("Please enter a valid venue name.")
            .Must(v => v.Trim().Length is >= EventConstants.MinVenueNameLength and <= EventConstants.MaxVenueNameLength)
            .WithMessage("Please enter a valid venue name.")
            .When(x => !string.IsNullOrEmpty(x.VenueName), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.VenueAddress)
            .NotEmpty().WithMessage("Please enter a valid venue address and location.")
            .Must(a => a.Trim().Length is >= EventConstants.MinVenueAddressLength and <= EventConstants.MaxVenueAddressLength)
            .WithMessage("Please enter a valid venue address and location.")
            .When(x => !string.IsNullOrEmpty(x.VenueAddress), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("Please select a valid location on the map.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("Please select a valid location on the map.");

        RuleFor(x => x.StartAtUtc)
            .Must(start => start >= DateTimeOffset.UtcNow + EventConstants.MinLeadTime)
            .WithMessage(EventConstants.StartMustBeFutureMessage);

        RuleFor(x => x.EndAtUtc)
            .GreaterThan(x => x.StartAtUtc)
            .WithMessage(EventConstants.EndMustBeAfterStartMessage);

        RuleFor(x => x.Capacity)
            .InclusiveBetween(EventConstants.MinCapacity, EventConstants.MaxCapacity)
            .WithMessage(EventConstants.CapacityRangeMessage);

        RuleFor(x => x.CoordinatorName)
            .NotEmpty().WithMessage("Please enter a valid coordinator name.")
            .Must(n => n.Trim().Length is >= EventConstants.MinCoordinatorNameLength and <= EventConstants.MaxCoordinatorNameLength)
            .WithMessage("Please enter a valid coordinator name.")
            .When(x => !string.IsNullOrEmpty(x.CoordinatorName), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.CoordinatorContact)
            .Matches(MobilePattern).WithMessage("Please enter a valid coordinator contact number.");

        RuleFor(x => x.RsvpCutoffAtUtc)
            .Must((request, cutoff) => cutoff is null || cutoff < request.StartAtUtc)
            .WithMessage(EventConstants.RsvpCutoffMustPrecedeStartMessage);
    }
}
