using FluentValidation;
using Howsee.Application.DTOs.requests.Pricing;

namespace Howsee.Application.Validators.Pricing;

public class UpdatePricingPlanRequestValidator : AbstractValidator<UpdatePricingPlanRequest>
{
    public UpdatePricingPlanRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0.")
            .When(x => x.Amount.HasValue);

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters.")
            .When(x => x.Currency != null);

        RuleFor(x => x.Unit)
            .MaximumLength(20).WithMessage("Unit must not exceed 20 characters.")
            .When(x => x.Unit != null);
    }
}
