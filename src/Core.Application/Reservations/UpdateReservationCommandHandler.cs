using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Reservations;
internal class UpdateReservationCommandHandler(IReservationsDatabase reservationsDatabase)
    : IRequestHandler<UpdateReservationCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(UpdateReservationCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Updating reservation {ReservationId}.", request.ReservationId);
        Log.Debug("Reservation data being saved is {@request}.", request);

        var reservationEntity = new ReservationEntityModel
        {
            Email = request.Email,
            Id = request.ReservationId,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = request.PreferredLanguage,
            SeatNumber = request.SeatNumber,
        };
        if (await reservationsDatabase.UpdateReservation(reservationEntity))
        {
            return Result.Updated;
        }

        Log.Warning("The reservation {ReservationId} could not be udpated because it does not exist.", request.ReservationId);
        return Error.NotFound();
    }
}
