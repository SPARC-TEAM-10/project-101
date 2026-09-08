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
    }
}
