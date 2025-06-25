namespace Core.Domain.Common.Enumerations;
public enum SeatStatus
{
    /// <summary>
    /// Seat is available for anyone to choose.
    /// </summary>
    Available,

    /// <summary>
    /// Seat is available to the single user with the key.
    /// </summary>
    Locked,

    /// <summary>
    /// Reservation is created but payment is not confirmed.
    /// </summary>
    AwaitingPayment,

    /// <summary>
    /// Staff has confirmed payment.
    /// </summary>
    ReservationConfirmed,
}
