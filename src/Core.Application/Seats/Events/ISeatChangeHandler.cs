using Core.Domain.Common.Enumerations;

namespace Core.Application.Seats.Events;
public interface ISeatChangeHandler
{
    Task OnSeatStatusChanged(int seatNumber, SeatStatus newStatus);
}
