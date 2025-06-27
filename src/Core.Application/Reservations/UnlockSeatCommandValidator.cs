using FluentValidation;

namespace Core.Application.Reservations;
public class UnlockSeatCommandValidator : AbstractValidator<UnlockSeatCommand>
{
    public UnlockSeatCommandValidator()
    {
        RuleFor(p => p.SeatKey)
            .Length(44)
            .NotEmpty();
    }
}
