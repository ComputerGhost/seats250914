using FluentValidation;

namespace Core.Application.Configuration;
public class SaveConfigurationValidator : AbstractValidator<SaveConfigurationCommand>
{
    public SaveConfigurationValidator()
    {
        RuleFor(p => p.ScheduledOpenTimeZone)
            .Must(BeValidTimeZone);

        RuleFor(p => p.ForceOpenReservations)
            .Must((command, value) => !(value & command.ForceCloseReservations))
            .WithMessage("Cannot both force open and force close reservations.");
    }

    public static bool BeValidTimeZone(string timeZoneId)
    {
        return TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var _);
    }
}
