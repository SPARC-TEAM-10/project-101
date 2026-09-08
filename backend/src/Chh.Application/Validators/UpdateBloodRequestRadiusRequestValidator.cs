using Chh.Application.Dtos;
using Chh.Domain.Constants;
using FluentValidation;

namespace Chh.Application.Validators;

/// <summary>
/// Validates <see cref="UpdateBloodRequestRadiusRequest"/> at the controller boundary (CHH-36
/// AC4). The "must actually be larger than the current radius" rule needs the existing request's
/// state, which this validator doesn't have — that check lives in
/// <c>BloodRequestService.UpdateRadiusAsync</c> instead (<c>RadiusMustIncreaseException</c>).
/// </summary>
public class UpdateBloodRequestRadiusRequestValidator : AbstractValidator<UpdateBloodRequestRadiusRequest>
{
    /// <summary>Configures the validation rules for <see cref="UpdateBloodRequestRadiusRequest"/>.</summary>
    public UpdateBloodRequestRadiusRequestValidator()
    {
        RuleFor(x => x.SearchRadiusKm)
            .GreaterThanOrEqualTo(BloodRequestConstants.MinSearchRadiusKm)
            .WithMessage(BloodRequestConstants.RadiusTooSmallMessage)
            .LessThanOrEqualTo(BloodRequestConstants.MaxSearchRadiusKm)
            .WithMessage(BloodRequestConstants.RadiusTooLargeMessage);
    }
}
