using FluentValidation;

namespace Core.Application.Reservations;
public class LockSeatCommandValidator : AbstractValidator<LockSeatCommand>
{
    public LockSeatCommandValidator()
    {
        RuleFor(p => p.IpAddress)
            .MaximumLength(45)
            .NotEmpty();
    }
}
