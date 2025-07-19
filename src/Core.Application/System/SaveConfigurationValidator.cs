using FluentValidation;

namespace Core.Application.System;
public class SaveConfigurationValidator : AbstractValidator<SaveConfigurationCommand>
{
    public SaveConfigurationValidator()
    {
        RuleFor(p => p.ForceOpenReservations)
            .Must((command, value) => !(value & command.ForceCloseReservations))
            .WithMessage("Cannot both force open and force close reservations.");

        RuleFor(p => p.MaxSeatsPerReservation)
            .LessThanOrEqualTo(10);

        RuleFor(p => p.ScheduledCloseTimeZone)
            .Must(BeValidTimeZone);

        RuleFor(p => p.ScheduledOpenTimeZone)
            .Must(BeValidTimeZone);
    }

    public static bool BeValidTimeZone(string timeZoneId)
    {
        return TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var _);
    }
}
