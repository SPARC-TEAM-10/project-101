using Chh.Application.Dtos;
using Chh.Application.Validators;
using Chh.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Chh.Application.Tests.Validators;

public class CreateFacilityRequestValidatorTests
{
    private readonly CreateFacilityRequestValidator _validator = new();

    private static CreateFacilityRequest ValidRequest() => new()
    {
        FacilityName = "City General Hospital",
        Category = FacilityCategory.Hospital,
        SubCategory = FacilitySubCategory.Government,
        LicenseNumber = "KL-HOSP-000000",
        Address = "123 Main St, Kochi",
        Contacts =
        [
            new CreateFacilityContactRequest { Name = "Jane Doe", Designation = "Administrator", Mobile = "9876543210" }
        ]
    };

    [Fact]
    public void Validate_WhenAllFieldsAreValid_HasNoValidationErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenFacilityNameIsTooShort_HasValidationErrorForFacilityName()
    {
        var request = ValidRequest() with { FacilityName = "AB" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.FacilityName);
    }

    [Fact]
    public void Validate_WhenSubCategoryDoesNotMatchCategory_HasValidationErrorForSubCategory()
    {
        var request = ValidRequest() with { Category = FacilityCategory.Hospital, SubCategory = FacilitySubCategory.RegisteredSociety };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.SubCategory);
    }

    [Theory]
    [InlineData(FacilitySubCategory.Government)]
    [InlineData(FacilitySubCategory.Private)]
    [InlineData(FacilitySubCategory.Trust)]
    public void Validate_WhenSubCategoryIsValidForHospital_HasNoValidationErrorForSubCategory(FacilitySubCategory subCategory)
    {
        var request = ValidRequest() with { Category = FacilityCategory.Hospital, SubCategory = subCategory };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.SubCategory);
    }

    [Theory]
    [InlineData(FacilitySubCategory.RegisteredSociety)]
    [InlineData(FacilitySubCategory.Trust)]
    [InlineData(FacilitySubCategory.Section8Company)]
    public void Validate_WhenSubCategoryIsValidForNgo_HasNoValidationErrorForSubCategory(FacilitySubCategory subCategory)
    {
        var request = ValidRequest() with { Category = FacilityCategory.Ngo, SubCategory = subCategory };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.SubCategory);
    }

    [Fact]
    public void Validate_WhenLicenseNumberHasInvalidCharacters_HasValidationErrorForLicenseNumber()
    {
        var request = ValidRequest() with { LicenseNumber = "KL/HOSP#123" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.LicenseNumber);
    }

    [Fact]
    public void Validate_WhenAddressIsEmpty_HasValidationErrorForAddress()
    {
        var request = ValidRequest() with { Address = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Validate_WhenNoContacts_HasValidationErrorForContacts()
    {
        var request = ValidRequest() with { Contacts = [] };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Contacts);
    }

    [Fact]
    public void Validate_WhenMoreThanThreeContacts_HasValidationErrorForContacts()
    {
        var contact = new CreateFacilityContactRequest { Name = "A", Designation = "B", Mobile = "9876543210" };
        var request = ValidRequest() with { Contacts = [contact, contact, contact, contact] };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Contacts);
    }

    [Fact]
    public void Validate_WhenContactMobileIsNotTenDigits_HasValidationErrorForContactMobile()
    {
        var request = ValidRequest() with
        {
            Contacts = [new CreateFacilityContactRequest { Name = "Jane", Designation = "Admin", Mobile = "12345" }]
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Contacts[0].Mobile");
    }
}
