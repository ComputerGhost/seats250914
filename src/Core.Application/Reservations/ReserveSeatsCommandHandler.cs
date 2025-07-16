using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
internal class ReserveSeatsCommandHandler : IRequestHandler<ReserveSeatCommand, ErrorOr<int>>
{
    public async Task<ErrorOr<int>> Handle(ReserveSeatCommand request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return 0;
    }
}
