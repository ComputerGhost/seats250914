using Core.Domain.Reservations;
using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
internal class RejectReservationCommandHandler(IReservationService reservationService) : IRequestHandler<RejectReservationCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RejectReservationCommand request, CancellationToken cancellationToken)
    {
        return await reservationService.RejectReservation(request.ReservationId)
            ? Result.Success
            : Error.NotFound();
    }
}
