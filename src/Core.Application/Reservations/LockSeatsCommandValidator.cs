using FluentValidation;

namespace Core.Application.Reservations;
public class LockSeatsCommandValidator : AbstractValidator<LockSeatsCommand>
{
    public LockSeatsCommandValidator()
    {
        RuleFor(p => p.IpAddress)
            .MaximumLength(45)
            .NotEmpty();

        RuleFor(p => p.SeatNumbers)
            .NotEmpty();
    }
}
