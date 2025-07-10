namespace Core.Domain.Common.Enumerations;
public enum EmailType
{
    /// <summary>
    /// User has submitted a reservation request.
    /// </summary>
    /// <remarks>
    /// When enqueueing this email type, the reference id should point to `Reservations`.
    /// </remarks>
    UserSubmittedReservation,
}
