using Core.Domain.Reservations;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Reservations;
internal class RejectReservationCommandHandler(IReservationService reservationService) : IRequestHandler<RejectReservationCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(RejectReservationCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Rejecting reservation {ReservationId}.", request.ReservationId);

        if (await reservationService.RejectReservation(request.ReservationId))
        {
            return Result.Success;
        }

        Log.Warning("Could not reject reservation {ReservationId} becuase it does not exist.", request.ReservationId);
        return Error.NotFound();
    }
}
