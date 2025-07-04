using FluentValidation;
using Gomotel.Controllers;

namespace Gomotel.Application.Validators;

public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(x => x.MotelId).NotEmpty().WithMessage("Motel ID is required");

        RuleFor(x => x.RoomId).NotEmpty().WithMessage("Room ID is required");

        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage("Start time is required")
            .Must(BeInFuture)
            .WithMessage("Start time must be in the future");

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .WithMessage("End time is required")
            .GreaterThan(x => x.StartTime)
            .WithMessage("End time must be after start time");

        RuleFor(x => x)
            .Must(HaveValidDuration)
            .WithMessage("Reservation duration must be between 1 hour and 24 hours");

        RuleFor(x => x.SpecialRequests)
            .MaximumLength(1000)
            .WithMessage("Special requests cannot exceed 1000 characters");
    }

    private static bool BeInFuture(DateTime startTime)
    {
        return startTime > DateTime.UtcNow.AddMinutes(-5); // Allow 5 minutes tolerance
    }

    private static bool HaveValidDuration(CreateReservationRequest request)
    {
        var duration = request.EndTime - request.StartTime;
        return duration.TotalHours >= 1 && duration.TotalHours <= 24;
    }
}
