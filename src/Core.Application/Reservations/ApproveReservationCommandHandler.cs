using Core.Domain.Reservations;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Reservations;
internal class ApproveReservationCommandHandler(IReservationService reservationService)
    : IRequestHandler<ApproveReservationCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(ApproveReservationCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Approving reservation {ReservationId}.", request.ReservationId);

        if (await reservationService.ApproveReservation(request.ReservationId))
        {
            return Result.Success;
        }

        Log.Information("Reservation {ReservationId} could not be approved because it does not exist.", request.ReservationId);
        return Error.NotFound();
    }
}
