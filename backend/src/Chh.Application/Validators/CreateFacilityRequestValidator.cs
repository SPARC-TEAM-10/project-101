using Chh.Application.Dtos;
using Chh.Domain.Constants;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="CreateFacilityContactRequest"/> — one contact entry (CHH-78/US-CHH-003-01 AC2).</summary>
public class CreateFacilityContactRequestValidator : AbstractValidator<CreateFacilityContactRequest>
{
    private const string MobilePattern = @"^\d{10}$";

    /// <summary>Configures the validation rules for one contact.</summary>
    public CreateFacilityContactRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Enter contact name");

        RuleFor(x => x.Designation)
            .NotEmpty().WithMessage("Enter designation");

        RuleFor(x => x.Mobile)
            .Matches(MobilePattern).WithMessage("Enter all 10 digits of the mobile number.");
    }
}

/// <summary>
/// Validates <see cref="CreateFacilityRequest"/> at the controller boundary (CHH-78/US-CHH-003-01).
/// </summary>
public class CreateFacilityRequestValidator : AbstractValidator<CreateFacilityRequest>
{
    private const int MinFacilityNameLength = 3;
    private const string LicenseNumberPattern = @"^[A-Za-z0-9-]+$";

    /// <summary>Configures the validation rules for <see cref="CreateFacilityRequest"/>.</summary>
    public CreateFacilityRequestValidator()
    {
        RuleFor(x => x.FacilityName)
            .NotEmpty().WithMessage("Enter facility name")
            .Must(name => name.Trim().Length >= MinFacilityNameLength)
            .WithMessage($"Facility name must be at least {MinFacilityNameLength} characters.")
            .When(x => !string.IsNullOrEmpty(x.FacilityName), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Select a category.");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("Enter license number")
            .Matches(LicenseNumberPattern)
            .WithMessage(FacilityConstants.InvalidLicenseNumberMessage)
            .When(x => !string.IsNullOrEmpty(x.LicenseNumber), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Enter address");

        RuleFor(x => x.Contacts)
            .NotEmpty().WithMessage("At least one contact is required.")
            .Must(contacts => contacts.Count <= FacilityConstants.MaxContacts)
            .WithMessage($"Three contacts is the maximum.")
            .Must(HaveNoDuplicateMobiles)
            .WithMessage(FacilityConstants.DuplicateContactMobileMessage);

        RuleForEach(x => x.Contacts)
            .SetValidator(new CreateFacilityContactRequestValidator());
    }

    private static bool HaveNoDuplicateMobiles(IReadOnlyList<CreateFacilityContactRequest> contacts)
    {
        var mobiles = contacts.Select(c => c.Mobile).Where(m => !string.IsNullOrEmpty(m)).ToList();
        return mobiles.Count == mobiles.Distinct().Count();
    }
}
