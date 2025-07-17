using FluentValidation;

namespace Core.Application.Reservations;
public class UnlockSeatsCommandValidator : AbstractValidator<UnlockSeatsCommand>
{
    public UnlockSeatsCommandValidator()
    {
        RuleFor(p => p.SeatLocks)
            .NotEmpty();
    }
}
