namespace Core.Application.Common.Enumerations;
public enum ReservationStatus
{
    /// <summary>
    /// Reservation is created but payment is not confirmed.
    /// </summary>
    AwaitingPayment,

    /// <summary>
    /// Staff has confirmed payment.
    /// </summary>
    ReservationConfirmed,

    /// <summary>
    /// Staff has rejected the reservation.
    /// </summary>
    ReservationRejected,
}
