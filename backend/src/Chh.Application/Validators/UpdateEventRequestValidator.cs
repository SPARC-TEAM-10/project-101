using Chh.Application.Dtos;
using Chh.Domain.Constants;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>
/// Validates <see cref="UpdateEventRequest"/> at the controller boundary (CHH-41/US-CHH-005-04,
/// spec §6.2) — per-field bounds only. Cross-field/entity-aware rules (end after start, capacity
/// vs. current RSVP count, started-event/ownership guards) need the loaded <c>Event</c> and live in
/// <see cref="Chh.Application.Services.EventService.UpdateAsync"/> instead.
/// </summary>
public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    private static readonly System.Text.RegularExpressions.Regex MobilePattern =
        new(@"^\d{10}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>Configures the validation rules for <see cref="UpdateEventRequest"/>.</summary>
    public UpdateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .Must(t => t!.Trim().Length is >= EventConstants.MinTitleLength and <= EventConstants.MaxTitleLength)
            .WithMessage($"Title must be between {EventConstants.MinTitleLength} and {EventConstants.MaxTitleLength} characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.EventType)
            .IsInEnum().WithMessage("Please select an event type.")
            .When(x => x.EventType is not null);

        RuleFor(x => x.Description)
            .Must(d => d!.Trim().Length is >= EventConstants.MinDescriptionLength and <= EventConstants.MaxDescriptionLength)
            .WithMessage($"Description must be between {EventConstants.MinDescriptionLength} and {EventConstants.MaxDescriptionLength} characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.VenueName)
            .Must(v => v!.Trim().Length is >= EventConstants.MinVenueNameLength and <= EventConstants.MaxVenueNameLength)
            .WithMessage("Please enter a valid venue name.")
            .When(x => x.VenueName is not null);

        RuleFor(x => x.VenueAddress)
            .Must(a => a!.Trim().Length is >= EventConstants.MinVenueAddressLength and <= EventConstants.MaxVenueAddressLength)
            .WithMessage("Please enter a valid venue address and location.")
            .When(x => x.VenueAddress is not null);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("Please select a valid location on the map.")
            .When(x => x.Latitude is not null);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("Please select a valid location on the map.")
            .When(x => x.Longitude is not null);

        RuleFor(x => x.Capacity)
            .InclusiveBetween(EventConstants.MinCapacity, EventConstants.MaxCapacity)
            .WithMessage(EventConstants.CapacityRangeMessage)
            .When(x => x.Capacity is not null);

        RuleFor(x => x.CoordinatorName)
            .Must(n => n!.Trim().Length is >= EventConstants.MinCoordinatorNameLength and <= EventConstants.MaxCoordinatorNameLength)
            .WithMessage("Please enter a valid coordinator name.")
            .When(x => x.CoordinatorName is not null);

        RuleFor(x => x.CoordinatorContact)
            .Matches(MobilePattern).WithMessage("Please enter a valid coordinator contact number.")
            .When(x => x.CoordinatorContact is not null);
    }
}
