using Core.Domain.Authorization;
using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Ports;
using Core.Domain.Reservations;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Reservations;
internal class ReserveSeatsCommandHandler : IRequestHandler<ReserveSeatsCommand, ErrorOr<int>>
{
    private readonly IAuthorizationChecker _authorizationChecker;
    private readonly IEmailsDatabase _emailsDatabase;
    private readonly IReservationService _reservationService;

    public ReserveSeatsCommandHandler(IAuthorizationChecker authorizationCheck, IEmailsDatabase emailsDatabase, IReservationService reservationService)
    {
        _authorizationChecker = authorizationCheck;
        _emailsDatabase = emailsDatabase;
        _reservationService = reservationService;
    }

    public async Task<ErrorOr<int>> Handle(ReserveSeatsCommand request, CancellationToken cancellationToken)
    {
        var seatNumbers = request.SeatLocks.Keys.ToList();
        Log.Information("Reserving seats {seatNumbers} for identity {@Identity}.", seatNumbers, request.Identity);

        var authResult = await _authorizationChecker.GetReserveSeatsAuthorization(request.Identity, request.SeatLocks);
        if (!authResult.IsAuthorized)
        {
            return Unauthorized(seatNumbers, authResult);
        }

        var reservationId = await _reservationService.ReserveSeats(seatNumbers, request.Identity);
        if (reservationId == null)
        {
            return UnauthorizedJustNow(seatNumbers);
        }

        Log.Information("Enqueueing email for reserved seats {seatNumbers} under reservation id {Value}.", seatNumbers, reservationId.Value);
        await _emailsDatabase.EnqueueEmail(EmailType.UserSubmittedReservation.ToString(), reservationId.Value);

        return reservationId.Value;
    }

    private static Error Unauthorized(IEnumerable<int> seatNumbers, AuthorizationResult authResult)
    {
        var reason = authResult.FailureReason.ToString();
        Log.Information("User is not authorized to reserve seats {seatNumbers} because {reason}.", seatNumbers, reason);
        return Error.Unauthorized(metadata: new Dictionary<string, object> { { "details", authResult } });
    }

    private static Error UnauthorizedJustNow(IEnumerable<int> seatNumbers)
    {
        Log.Warning("User failed to reserve seats {seatNumbers}. Their seat keys probably expired just now.", seatNumbers);
        return Error.Unauthorized(metadata: new Dictionary<string, object> {
            { "details", AuthorizationResult.KeyIsExpired }
        });
    }
}
