using Core.Domain.Common.Models;

namespace Core.Domain.Reservations;
public interface IReservationService
{
    Task<bool> ApproveReservation(int reservationId);

    Task<bool> RejectReservation(int reservationId);

    Task<int?> ReserveSeat(int seatNumber, IdentityModel identity);
}
