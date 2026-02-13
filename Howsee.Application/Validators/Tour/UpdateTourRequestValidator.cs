using FluentValidation;
using Howsee.Application.DTOs.requests.Tour;

namespace Howsee.Application.Validators.Tour;

public class UpdateTourRequestValidator : AbstractValidator<UpdateTourRequest>
{
    public UpdateTourRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => x.Title != null);

        RuleFor(x => x.MatterportModelId)
            .MaximumLength(100).WithMessage("Matterport model ID must not exceed 100 characters.")
            .When(x => x.MatterportModelId != null);

        RuleFor(x => x.StartSweepId)
            .MaximumLength(100).WithMessage("Start sweep ID must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.StartSweepId));
    }
}
