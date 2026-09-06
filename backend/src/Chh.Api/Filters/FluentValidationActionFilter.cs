using Chh.Application.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Chh.Api.Filters;

/// <summary>
/// Runs the DI-registered FluentValidation <see cref="IValidator{T}"/> (if any) against each
/// action argument and throws <see cref="ChhValidationException"/> — mapped to 422 with
/// per-field failures in <c>ProblemDetailsServiceCollectionExtensions</c> — on failure.
/// </summary>
/// <remarks>
/// Replaces <c>FluentValidation.AspNetCore</c>'s auto-validation, which short-circuits with its
/// own <c>BadRequestObjectResult</c> (400) directly from an action filter — it never goes
/// through <c>ApiBehaviorOptions.InvalidModelStateResponseFactory</c>, so that override never
/// actually ran for FluentValidation failures, leaving every validated endpoint returning 400
/// instead of the 422 required by api-standards.md §7.
/// </remarks>
public class FluentValidationActionFilter : IAsyncActionFilter
{
    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (!result.IsValid)
            {
                var failures = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                throw new ChhValidationException("Validation failed", failures);
            }
        }

        await next();
    }
}
