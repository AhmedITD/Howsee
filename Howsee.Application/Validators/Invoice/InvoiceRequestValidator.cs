using FluentValidation;
using Howsee.Application.DTOs.requests.Invoice;

namespace Howsee.Application.Validators.Invoice;

public class InvoiceRequestValidator : AbstractValidator<InvoiceRequest>
{
    public InvoiceRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.");

        RuleFor(x => x.FinishUrl)
            .NotEmpty().WithMessage("Finish URL is required.");
    }
}
