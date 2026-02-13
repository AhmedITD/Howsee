using FluentValidation;
using Howsee.Application.DTOs.requests.Pricing;

namespace Howsee.Application.Validators.Pricing;

public class CreatePricingPlanRequestValidator : AbstractValidator<CreatePricingPlanRequest>
{
    public CreatePricingPlanRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required.")
            .MaximumLength(100).WithMessage("Key must not exceed 100 characters.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount must be greater than or equal to 0.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(20).WithMessage("Unit must not exceed 20 characters.");
    }
}
