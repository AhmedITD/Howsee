using FluentValidation;
using Howsee.Application.DTOs.requests.Auth;

namespace Howsee.Application.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.VerificationCode).NotEmpty().MaximumLength(10);
    }
}
