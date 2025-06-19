using Core.Application.Seats;
using Core.Application.Common.Enumerations;
using Public.Views.Shared.Components.Enumerations;

namespace Public.Features.SeatSelection.Models;

public class SeatSelectorViewModel
{
    public SeatSelectorViewModel(ListSeatsQueryResponse listSeatsQueryResponse)
    {
        bool areOnHoldSeats = false;
        bool areAvailableSeats = false;

        foreach (var seat in listSeatsQueryResponse.Data)
        {
            SeatStatuses.Add(seat.SeatNumber, StatusEnumToString(seat.Status));

            areOnHoldSeats = areOnHoldSeats
                || seat.Status == SeatStatus.Locked
                || seat.Status == SeatStatus.AwaitingPayment;

            areAvailableSeats = areAvailableSeats
                || seat.Status == SeatStatus.Available;
        }

        if (areAvailableSeats)
        {
            SystemStatus = SystemStatus.Open;
        }
        else if (areOnHoldSeats)
        {
            SystemStatus = SystemStatus.OutOfSeatsTemporarily;
        }
        else
        {
            SystemStatus = SystemStatus.OutOfSeatsPermanently;
        }
    }

    public required string IdPrefix { get; init; }

    public bool IsOpen => SystemStatus == SystemStatus.Open;

    public required string LockSeatUrl { get; set; }

    public required string ReservationPageUrl { get; set; }

    public IDictionary<int, string> SeatStatuses { get; set; } = new Dictionary<int, string>();

    public SystemStatus SystemStatus { get; init; }

    // TODO: open time

    // TODO: close time

    private static string StatusEnumToString(SeatStatus statusEnum)
    {
        return statusEnum switch
        {
            SeatStatus.Available => "available",
            SeatStatus.Locked => "on-hold",
            SeatStatus.AwaitingPayment => "on-hold",
            SeatStatus.ReservationConfirmed => "reserved",
            _ => throw new NotImplementedException()
        };
    }
}
