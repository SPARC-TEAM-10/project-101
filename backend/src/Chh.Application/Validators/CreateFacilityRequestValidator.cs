using Chh.Application.Dtos;
using Chh.Domain.Constants;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>Validates <see cref="CreateFacilityRequest"/> at the controller boundary (CHH-78).</summary>
public class CreateFacilityRequestValidator : AbstractValidator<CreateFacilityRequest>
{
    private const int MinFacilityNameLength = 3;
    private const int MaxFacilityNameLength = 200;
    private const int MaxAddressLength = 500;
    private const int MinContacts = 1;
    private const int MaxContacts = 3;

    private static readonly System.Text.RegularExpressions.Regex LicenseNumberPattern =
        new("^[A-Za-z0-9-]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex MobilePattern =
        new(@"^\d{10}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>Configures the validation rules for <see cref="CreateFacilityRequest"/>.</summary>
    public CreateFacilityRequestValidator()
    {
        RuleFor(x => x.FacilityName)
            .NotEmpty().WithMessage("Facility name is required.")
            .Must(name => name.Trim().Length >= MinFacilityNameLength)
            .WithMessage($"Facility name must be at least {MinFacilityNameLength} characters.")
            .When(x => !string.IsNullOrEmpty(x.FacilityName))
            .MaximumLength(MaxFacilityNameLength);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Select a category.");

        RuleFor(x => x.SubCategory)
            .IsInEnum().WithMessage(FacilityConstants.SubCategoryRequiredMessage)
            .Must((request, subCategory) => FacilityConstants.SubCategoriesFor(request.Category).Contains(subCategory))
            .WithMessage(FacilityConstants.SubCategoryDoesNotMatchCategoryMessage);

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("Enter licence number.")
            .Matches(LicenseNumberPattern).WithMessage("Licence number can contain letters, numbers and hyphens only.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Enter address.")
            .MaximumLength(MaxAddressLength);

        RuleFor(x => x.Contacts)
            .Must(contacts => contacts.Count is >= MinContacts and <= MaxContacts)
            .WithMessage($"Add between {MinContacts} and {MaxContacts} contacts.");

        RuleForEach(x => x.Contacts).ChildRules(contact =>
        {
            contact.RuleFor(c => c.Name).NotEmpty().WithMessage("Enter contact name.");
            contact.RuleFor(c => c.Designation).NotEmpty().WithMessage("Enter designation.");
            contact.RuleFor(c => c.Mobile)
                .Matches(MobilePattern).WithMessage("Enter all 10 digits of the mobile number.");
        });
    }
}
