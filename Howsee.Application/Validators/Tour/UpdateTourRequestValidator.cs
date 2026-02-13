using FluentValidation;
using Howsee.Application.DTOs.requests.Tour;

namespace Howsee.Application.Validators.Tour;

public class UpdateTourRequestValidator : AbstractValidator<UpdateTourRequest>
{
    public UpdateTourRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title != null);
        RuleFor(x => x.MatterportModelId).MaximumLength(100).When(x => x.MatterportModelId != null);
        RuleFor(x => x.StartSweepId).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.StartSweepId));
    }
}
