using FluentValidation;
using Howsee.Application.DTOs.requests.Invoice;

namespace Howsee.Application.Validators.Invoice;

public class InvoiceRequestValidator : AbstractValidator<InvoiceRequest>
{
    public InvoiceRequestValidator()
    {
        RuleFor(x => x.PlanKey)
            .NotEmpty().WithMessage("Plan key is required.");

        RuleFor(x => x.FinishUrl)
            .NotEmpty().WithMessage("Finish URL is required.");
    }
}
