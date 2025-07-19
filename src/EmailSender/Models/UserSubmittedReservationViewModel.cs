using Core.Application.Reservations;

namespace EmailSender.Models;
internal class UserSubmittedReservationViewModel
{
    public UserSubmittedReservationViewModel(FetchReservationQueryResponse queryResponse)
    {
        Name = queryResponse.Name;
        HasMultipleSeats = queryResponse.SeatNumbers.Count() > 1;
        SeatNumbers = string.Join(",", queryResponse.SeatNumbers.Order());
    }

    /// <summary>
    /// Name of the person reserving the seat.
    /// </summary>
    public string Name { get; set; } = null!;

    public bool HasMultipleSeats { get; set; }

    public string SeatNumbers { get; set; } = null!;
}
