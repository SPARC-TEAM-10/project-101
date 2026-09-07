using Chh.Application.Dtos;
using Chh.Application.Validators;
using Chh.Domain.Constants;
using Chh.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Chh.Application.Tests.Validators;

public class CreateFacilityRequestValidatorTests
{
    private readonly CreateFacilityRequestValidator _validator = new();

    private static CreateFacilityContactRequest ValidContact(string mobile = "9876500112") => new()
    {
        Name = "Anitha Varghese",
        Designation = "Blood bank officer",
        Mobile = mobile
    };

    private static CreateFacilityRequest ValidRequest() => new()
    {
        FacilityName = "Kochi Metro Hospital",
        Category = FacilityCategory.Hospital,
        LicenseNumber = "KL-HOSP-448120",
        Address = "4th Block, Marine Drive, Ernakulam, Kochi 682031",
        Contacts = [ValidContact()]
    };

    [Fact]
    public void Validate_WhenAllFieldsAreValid_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenFacilityNameIsUnderMinLength_HasValidationErrorForFacilityName()
    {
        var request = ValidRequest() with { FacilityName = "St" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FacilityName)
            .WithErrorMessage("Facility name must be at least 3 characters.");
    }

    [Fact]
    public void Validate_WhenFacilityNameIsEmpty_HasValidationErrorForFacilityName()
    {
        var request = ValidRequest() with { FacilityName = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FacilityName);
    }

    [Fact]
    public void Validate_WhenCategoryIsOutsideEnum_HasValidationErrorForCategory()
    {
        var request = ValidRequest() with { Category = (FacilityCategory)99 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Select a category.");
    }

    [Theory]
    [InlineData("KL/HOSP/2019")]
    [InlineData("KL HOSP 2019")]
    public void Validate_WhenLicenseNumberHasInvalidCharacters_HasValidationErrorForLicenseNumber(string licenseNumber)
    {
        var request = ValidRequest() with { LicenseNumber = licenseNumber };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.LicenseNumber)
            .WithErrorMessage(FacilityConstants.InvalidLicenseNumberMessage);
    }

    [Fact]
    public void Validate_WhenAddressIsEmpty_HasValidationErrorForAddress()
    {
        var request = ValidRequest() with { Address = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Validate_WhenContactsIsEmpty_HasValidationErrorForContacts()
    {
        var request = ValidRequest() with { Contacts = [] };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Contacts)
            .WithErrorMessage("At least one contact is required.");
    }

    [Fact]
    public void Validate_WhenContactsExceedsMax_HasValidationErrorForContacts()
    {
        var request = ValidRequest() with
        {
            Contacts =
            [
                ValidContact("9876500001"),
                ValidContact("9876500002"),
                ValidContact("9876500003"),
                ValidContact("9876500004")
            ]
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Contacts)
            .WithErrorMessage(FacilityConstants.TooManyContactsMessage);
    }

    [Fact]
    public void Validate_WhenContactsAtMax_HasNoValidationErrorForContacts()
    {
        var request = ValidRequest() with
        {
            Contacts =
            [
                ValidContact("9876500001"),
                ValidContact("9876500002"),
                ValidContact("9876500003")
            ]
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Contacts);
    }

    [Fact]
    public void Validate_WhenTwoContactsShareAMobileNumber_HasValidationErrorForContacts()
    {
        var request = ValidRequest() with
        {
            Contacts = [ValidContact("9876500112"), ValidContact("9876500112")]
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Contacts)
            .WithErrorMessage(FacilityConstants.DuplicateContactMobileMessage);
    }

    [Fact]
    public void Validate_WhenAContactNameIsEmpty_HasValidationErrorForThatContact()
    {
        var request = ValidRequest() with { Contacts = [ValidContact() with { Name = "" }] };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Contacts[0].Name");
    }

    [Fact]
    public void Validate_WhenAContactDesignationIsEmpty_HasValidationErrorForThatContact()
    {
        var request = ValidRequest() with { Contacts = [ValidContact() with { Designation = "" }] };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Contacts[0].Designation");
    }

    [Theory]
    [InlineData("98765")]
    [InlineData("abcdefghij")]
    [InlineData("98765001123")]
    public void Validate_WhenAContactMobileIsNotTenDigits_HasValidationErrorForThatContact(string mobile)
    {
        var request = ValidRequest() with { Contacts = [ValidContact(mobile)] };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Contacts[0].Mobile");
    }
}
