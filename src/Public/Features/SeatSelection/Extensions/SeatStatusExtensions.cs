using Core.Domain.Common.Enumerations;

namespace Public.Features.SeatSelection.Extensions;

public static class SeatStatusExtensions
{
    public static string ToCssClass(this SeatStatus status)
    {
        return status switch
        {
            SeatStatus.Available => "available",
            SeatStatus.Locked => "on-hold",
            SeatStatus.AwaitingPayment => "on-hold",
            SeatStatus.ReservationConfirmed => "reserved",
            _ => throw new NotImplementedException()
        };
    }
}
