using Core.Domain.Common.Models;

namespace Core.Domain.Reservations;
public interface IReservationService
{
    Task<bool> ApproveReservation(int reservationId);

    Task<bool> RejectReservation(int reservationId);

    Task<int?> ReserveSeats(IList<int> seatNumbers, IdentityModel identity);
}
