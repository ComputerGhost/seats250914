using Core.Application.Seats.Enumerations;

namespace Core.Application.Seats;
public class ListSeatsQueryResponseItem
{
    public int SeatNumber { get; set; }

    public SeatStatus Status { get; set; }
}
