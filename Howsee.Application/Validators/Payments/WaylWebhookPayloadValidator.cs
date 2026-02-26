using FluentValidation;
using Howsee.Application.DTOs.requests.Payments;

namespace Howsee.Application.Validators.Payments;

public class WaylWebhookPayloadValidator : AbstractValidator<WaylWebhookPayload>
{
    public WaylWebhookPayloadValidator()
    {
        RuleFor(x => x.ReferenceId)
            .NotEmpty().WithMessage("ReferenceId is required.");

        RuleFor(x => x.ReferenceId)
            .Must(BeValidGuid)
            .WithMessage("ReferenceId must be a valid GUID.")
            .When(x => !string.IsNullOrWhiteSpace(x.ReferenceId));
    }

    private static bool BeValidGuid(string? value)
    {
        return Guid.TryParse(value, out _);
    }
}
