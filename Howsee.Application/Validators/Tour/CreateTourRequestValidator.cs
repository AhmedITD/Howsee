using FluentValidation;
using Howsee.Application.DTOs.requests.Tour;

namespace Howsee.Application.Validators.Tour;

public class CreateTourRequestValidator : AbstractValidator<CreateTourRequest>
{
    public CreateTourRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MatterportModelId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StartSweepId).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.StartSweepId));
    }
}
