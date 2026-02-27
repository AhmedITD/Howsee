using FluentValidation;
using Howsee.Application.DTOs.requests.PropertyCategory;

namespace Howsee.Application.Validators.PropertyCategory;

public class CreatePropertyCategoryRequestValidator : AbstractValidator<CreatePropertyCategoryRequest>
{
    public CreatePropertyCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
    }
}
