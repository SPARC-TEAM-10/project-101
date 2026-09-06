using Chh.Application.Dtos;
using Chh.Domain.Constants;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="CreateIndividualProfileRequest"/> at the controller boundary (CHH-F02 §6.2).</summary>
public class CreateIndividualProfileRequestValidator : AbstractValidator<CreateIndividualProfileRequest>
{
    private const int MinFullNameLength = 2;
    private const int MaxFullNameLength = 50;
    private const int MaxOtherIllnessDetailsLength = 200;
    private const int MinimumAgeYears = 18;

    /// <summary>Configures the validation rules for <see cref="CreateIndividualProfileRequest"/>.</summary>
    public CreateIndividualProfileRequestValidator()
    {
        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage(OtpConstants.InvalidMobileNumberMessage)
            .Matches(OtpConstants.MobileNumberPattern).WithMessage(OtpConstants.InvalidMobileNumberMessage);

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Please enter your full name")
            .Must(name => name.Trim().Length is >= MinFullNameLength and <= MaxFullNameLength)
            .WithMessage($"Name must be between {MinFullNameLength} and {MaxFullNameLength} characters")
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Enter a valid email address")
            .EmailAddress().WithMessage("Enter a valid email address");

        RuleFor(x => x.BloodGroup)
            .IsInEnum().WithMessage("Please select your blood group");

        RuleFor(x => x.DateOfBirth)
            .Must(BeAValidPastDate).WithMessage("Enter a valid date of birth")
            .Must(BeAtLeastMinimumAge).WithMessage($"You must be {MinimumAgeYears} or older to register")
            .When(x => x.DateOfBirth != default);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Please select your gender");

        RuleFor(x => x.LocationCityArea)
            .NotEmpty().WithMessage("Please select your location");

        RuleFor(x => x.OtherIllnessDetails)
            .NotEmpty().WithMessage("Please specify other illness")
            .MaximumLength(MaxOtherIllnessDetailsLength)
            .When(x => x.IsOtherIllness);
    }

    private static bool BeAValidPastDate(DateOnly dateOfBirth) =>
        dateOfBirth <= DateOnly.FromDateTime(DateTime.UtcNow);

    private static bool BeAtLeastMinimumAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        return age >= MinimumAgeYears;
    }
}
