using FluentValidation;
using Howsee.Application.DTOs.requests.Currency;

namespace Howsee.Application.Validators.Currency;

public class UpdateCurrencyRequestValidator : AbstractValidator<UpdateCurrencyRequest>
{
    public UpdateCurrencyRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Symbol)
            .MaximumLength(10).WithMessage("Symbol must not exceed 10 characters.")
            .When(x => !string.IsNullOrEmpty(x.Symbol));
    }
}
