using Core.Domain.Reservations;
using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
internal class ApproveReservationCommandHandler(IReservationService reservationService) : IRequestHandler<ApproveReservationCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ApproveReservationCommand request, CancellationToken cancellationToken)
    {
        return await reservationService.ApproveReservation(request.ReservationId)
            ? Result.Success
            : Error.NotFound();
    }
}
