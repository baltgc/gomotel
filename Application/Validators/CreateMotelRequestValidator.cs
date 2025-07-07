using FluentValidation;
using Gomotel.Controllers;

namespace Gomotel.Application.Validators;

public class CreateMotelRequestValidator : AbstractValidator<CreateMotelRequest>
{
    public CreateMotelRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Motel name is required")
            .Length(2, 200)
            .WithMessage("Motel name must be between 2 and 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .Length(10, 1000)
            .WithMessage("Description must be between 10 and 1000 characters");

        RuleFor(x => x.Street)
            .NotEmpty()
            .WithMessage("Street address is required")
            .Length(5, 200)
            .WithMessage("Street address must be between 5 and 200 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .Length(2, 100)
            .WithMessage("City must be between 2 and 100 characters");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required")
            .Length(2, 100)
            .WithMessage("State must be between 2 and 100 characters");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .WithMessage("ZIP code is required")
            .Matches(@"^\d{5}(-\d{4})?$")
            .WithMessage("ZIP code must be in format 12345 or 12345-6789");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required")
            .Length(2, 100)
            .WithMessage("Country must be between 2 and 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(@"^\+?[\d\s\-\(\)]{10,20}$")
            .WithMessage("Phone number must be valid");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email must be valid");

        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("Owner ID is required");

        RuleFor(x => x.ImageUrl)
            .Must(BeValidUrl)
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Image URL must be valid");
    }

    private static bool BeValidUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
