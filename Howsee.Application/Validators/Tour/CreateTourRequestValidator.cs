using FluentValidation;
using Howsee.Application.DTOs.requests.Tour;

namespace Howsee.Application.Validators.Tour;

public class CreateTourRequestValidator : AbstractValidator<CreateTourRequest>
{
    public CreateTourRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

        RuleFor(x => x.MatterportModelId)
            .NotEmpty().WithMessage("Matterport model ID is required.")
            .MaximumLength(100).WithMessage("Matterport model ID must not exceed 100 characters.");

        RuleFor(x => x.StartSweepId)
            .MaximumLength(100).WithMessage("Start sweep ID must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.StartSweepId));
    }
}
