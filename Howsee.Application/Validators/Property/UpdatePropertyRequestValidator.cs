using FluentValidation;
using Howsee.Application.DTOs.requests.Property;

namespace Howsee.Application.Validators.Property;

public class UpdatePropertyRequestValidator : AbstractValidator<UpdatePropertyRequest>
{
    public UpdatePropertyRequestValidator()
    {
        RuleFor(x => x.Category).IsInEnum().When(x => x.Category.HasValue);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description != null);
        RuleFor(x => x.Area).GreaterThanOrEqualTo(0).When(x => x.Area.HasValue);
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90).When(x => x.Lat.HasValue);
        RuleFor(x => x.Lng).InclusiveBetween(-180, 180).When(x => x.Lng.HasValue);
        RuleFor(x => x.Address).MaximumLength(200).When(x => x.Address != null);
        RuleFor(x => x.Locality).MaximumLength(100).When(x => x.Locality != null);
        RuleFor(x => x.AdministrativeArea).MaximumLength(100).When(x => x.AdministrativeArea != null);
        RuleFor(x => x.CountryCode).MaximumLength(10).When(x => x.CountryCode != null);
        RuleFor(x => x.PostalCode).MaximumLength(20).When(x => x.PostalCode != null);
    }
}
