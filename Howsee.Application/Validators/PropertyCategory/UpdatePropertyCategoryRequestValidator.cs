using FluentValidation;
using Howsee.Application.DTOs.requests.PropertyCategory;

namespace Howsee.Application.Validators.PropertyCategory;

public class UpdatePropertyCategoryRequestValidator : AbstractValidator<UpdatePropertyCategoryRequest>
{
    public UpdatePropertyCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required when provided.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));
    }
}
