using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
internal class UpdateReservationCommandHandler : IRequestHandler<UpdateReservationCommand, ErrorOr<Success>>
{
    private readonly IReservationsDatabase _reservationsDatabase;

    public UpdateReservationCommandHandler(IReservationsDatabase reservationsDatabase)
    {
        _reservationsDatabase = reservationsDatabase;
    }

    public async Task<ErrorOr<Success>> Handle(UpdateReservationCommand request, CancellationToken cancellationToken)
    {
        var reservationEntity = new ReservationEntityModel
        {
            Email = request.Email,
            Id = request.ReservationId,
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = request.PreferredLanguage,
            SeatNumber = request.SeatNumber,
        };
        var result = await _reservationsDatabase.UpdateReservation(reservationEntity);
        return result ? Result.Success : Error.NotFound();
    }
}
