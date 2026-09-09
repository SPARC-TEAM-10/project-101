using Chh.Application.Dtos;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="UpdateIndividualProfileRequest"/> at the controller boundary (CHH-F02 profile edit).</summary>
public class UpdateIndividualProfileRequestValidator : AbstractValidator<UpdateIndividualProfileRequest>
{
    private const int MaxOtherIllnessDetailsLength = 200;

    /// <summary>Configures the validation rules for <see cref="UpdateIndividualProfileRequest"/>.</summary>
    public UpdateIndividualProfileRequestValidator()
    {
        RuleFor(x => x.LocationCityArea)
            .NotEmpty().WithMessage("Please select your location");

        RuleFor(x => x.OtherIllnessDetails)
            .NotEmpty().WithMessage("Please specify other illness")
            .MaximumLength(MaxOtherIllnessDetailsLength)
            .When(x => x.IsOtherIllness);

        // Device-supplied coordinates (CHH-84) — both or neither, same reasoning as
        // CreateBloodRequestRequestValidator: no server-side geocoding fallback exists.
        RuleFor(x => x.Latitude)
            .NotNull().WithMessage("Latitude and longitude must be provided together.")
            .When(x => x.Longitude is not null);

        RuleFor(x => x.Longitude)
            .NotNull().WithMessage("Latitude and longitude must be provided together.")
            .When(x => x.Latitude is not null);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("Latitude must be between -90 and 90.")
            .When(x => x.Latitude is not null);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("Longitude must be between -180 and 180.")
            .When(x => x.Longitude is not null);
    }
}
