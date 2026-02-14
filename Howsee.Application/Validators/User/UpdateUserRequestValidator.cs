using FluentValidation;
using Howsee.Application.DTOs.requests.User;

namespace Howsee.Application.Validators.User;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.")
            .When(x => x.FullName != null);

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("Phone must not exceed 50 characters.")
            .When(x => x.Phone != null);

        RuleFor(x => x.NewPassword)
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.NewPassword));
    }
}
