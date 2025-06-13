namespace Core.Domain.Authorization;
public enum AuthorizationRejectionReason
{
    KeyIsInvalid,
    KeyIsExpired,
    ReservationsAreClosed,
    SeatIsNotLocked,
    TooManyReservationsForEmail,
    TooManySeatLocksForIpAddress,
}
