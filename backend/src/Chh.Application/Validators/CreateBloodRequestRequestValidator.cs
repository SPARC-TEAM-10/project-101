using Chh.Application.Dtos;
using Chh.Domain.Constants;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>
/// Validates <see cref="CreateBloodRequestRequest"/> at the controller boundary (CHH-33/US-CHH-004-01).
/// </summary>
/// <remarks>
/// Latitude/Longitude are required, device-supplied coordinates (browser/device Geolocation),
/// not server-side geocoding — there's no maps/geocoding API key configured yet (see
/// backend/CLAUDE.md's Tech Stack row: "Maps / Geo API — per PRD §11 — not yet decided"). A
/// missing pair is the story's documented Edge Case ("Requester location coordinates cannot be
/// resolved"), so it's rejected here rather than silently persisted as zero/null.
/// </remarks>
public class CreateBloodRequestRequestValidator : AbstractValidator<CreateBloodRequestRequest>
{
    private const int MinPatientNameLength = 2;
    private const int MaxPatientNameLength = 100;
    private const int MaxLocationCityAreaLength = 100;

    /// <summary>Configures the validation rules for <see cref="CreateBloodRequestRequest"/>.</summary>
    public CreateBloodRequestRequestValidator()
    {
        RuleFor(x => x.PatientName)
            .NotEmpty().WithMessage("Please enter the patient's name")
            .Must(name => name.Trim().Length is >= MinPatientNameLength and <= MaxPatientNameLength)
            .WithMessage($"Patient name must be between {MinPatientNameLength} and {MaxPatientNameLength} characters")
            .When(x => !string.IsNullOrEmpty(x.PatientName), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.BloodGroup)
            .IsInEnum().WithMessage("Please select a blood group");

        RuleFor(x => x.UnitsRequired)
            .GreaterThan(0).WithMessage("Units required must be at least 1");

        RuleFor(x => x.LocationCityArea)
            .NotEmpty().WithMessage("Please enter a location")
            .MaximumLength(MaxLocationCityAreaLength);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage(BloodRequestConstants.LocationNotResolvableMessage);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage(BloodRequestConstants.LocationNotResolvableMessage);

        RuleFor(x => x.SearchRadiusKm)
            .GreaterThanOrEqualTo(BloodRequestConstants.MinSearchRadiusKm)
            .WithMessage(BloodRequestConstants.RadiusTooSmallMessage)
            .LessThanOrEqualTo(BloodRequestConstants.MaxSearchRadiusKm)
            .WithMessage(BloodRequestConstants.RadiusTooLargeMessage);

        RuleFor(x => x.Urgency)
            .IsInEnum().WithMessage("Please select an urgency level");
    }
}
