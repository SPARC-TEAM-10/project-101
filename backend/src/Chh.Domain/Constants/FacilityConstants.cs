using Chh.Domain.Enums;

namespace Chh.Domain.Constants;

/// <summary>Facility-registration-related constants (CHH-78) — valid category/sub-category pairing, user-facing messages.</summary>
public static class FacilityConstants
{
    /// <summary>Sub-categories a <see cref="FacilityCategory.Hospital"/> facility may pick.</summary>
    public static readonly IReadOnlySet<FacilitySubCategory> HospitalSubCategories = new HashSet<FacilitySubCategory>
    {
        FacilitySubCategory.Government,
        FacilitySubCategory.Private,
        FacilitySubCategory.Trust
    };

    /// <summary>Sub-categories a <see cref="FacilityCategory.Ngo"/> facility may pick.</summary>
    public static readonly IReadOnlySet<FacilitySubCategory> NgoSubCategories = new HashSet<FacilitySubCategory>
    {
        FacilitySubCategory.RegisteredSociety,
        FacilitySubCategory.Trust,
        FacilitySubCategory.Section8Company
    };

    /// <summary>
    /// Sub-categories a <see cref="FacilityCategory.Ambulance"/> facility may pick (CHH-82/Epic
    /// CHH-68) — reuses Government/Private rather than introducing dedicated values, since
    /// ambulance self-registration doesn't exist yet.
    /// </summary>
    public static readonly IReadOnlySet<FacilitySubCategory> AmbulanceSubCategories = new HashSet<FacilitySubCategory>
    {
        FacilitySubCategory.Government,
        FacilitySubCategory.Private
    };

    /// <summary>Validation message when the mandatory sub-category is missing.</summary>
    public const string SubCategoryRequiredMessage = "Select a sub-category.";

    /// <summary>Validation message when the sub-category doesn't belong to the selected category.</summary>
    public const string SubCategoryDoesNotMatchCategoryMessage = "This sub-category isn't valid for the selected category.";

    /// <summary>User-facing message when a facility already exists for the given license number.</summary>
    public const string AlreadyRegisteredMessage = "A facility is already registered with this licence number";

    /// <summary>Returns the allowed sub-categories for <paramref name="category"/>.</summary>
    public static IReadOnlySet<FacilitySubCategory> SubCategoriesFor(FacilityCategory category) =>
        category switch
        {
            FacilityCategory.Hospital => HospitalSubCategories,
            FacilityCategory.Ambulance => AmbulanceSubCategories,
            _ => NgoSubCategories
        };
}
