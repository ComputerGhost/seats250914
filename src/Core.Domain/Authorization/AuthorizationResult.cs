namespace Core.Domain.Authorization;
public struct AuthorizationResult
{
    public static AuthorizationResult KeyIsInvalid => Failure(AuthorizationRejectionReason.KeyIsInvalid);
    public static AuthorizationResult KeyIsExpired => Failure(AuthorizationRejectionReason.KeyIsExpired);
    public static AuthorizationResult ReservationsAreClosed => Failure(AuthorizationRejectionReason.ReservationsAreClosed);
    public static AuthorizationResult SeatIsNotLocked => Failure(AuthorizationRejectionReason.SeatIsNotLocked);
    public static AuthorizationResult TooManyReservationsForEmail => Failure(AuthorizationRejectionReason.TooManyReservationsForEmail);
    public static AuthorizationResult TooManySeatLocksForIpAddress => Failure(AuthorizationRejectionReason.TooManySeatLocksForIpAddress);

    public static AuthorizationResult Success => new() { IsAuthorized = true, };

    public static AuthorizationResult Failure(AuthorizationRejectionReason reason) => new()
    {
        IsAuthorized = false,
        FailureReason = reason,
    };

    public bool IsAuthorized { get; set; }

    public AuthorizationRejectionReason? FailureReason { get; set; }
}
