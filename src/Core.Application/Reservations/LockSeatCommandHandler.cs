using Core.Application.Seats.Enumerations;
using Core.Domain.Authentication;
using Core.Domain.Authorization;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using System.Diagnostics;

namespace Core.Application.Reservations;
internal class LockSeatCommandHandler : IRequestHandler<LockSeatCommand, ErrorOr<LockSeatCommandResponse>>
{
    private readonly IConfigurationDatabase _configurationDatabase;
    private readonly IReservationAuthorizationChecker _authorizationCheck;
    private readonly ISeatLocksDatabase _seatLocksDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public LockSeatCommandHandler(
        IConfigurationDatabase configurationDatabase,
        IReservationAuthorizationChecker authorizationCheck,
        ISeatLocksDatabase seatLocksDatabase,
        ISeatsDatabase seatsDatabase)
    {
        _configurationDatabase = configurationDatabase;
        _authorizationCheck = authorizationCheck;
        _seatLocksDatabase = seatLocksDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task<ErrorOr<LockSeatCommandResponse>> Handle(LockSeatCommand request, CancellationToken cancellationToken)
    {
        var configuration = await _configurationDatabase.FetchConfiguration();
        var expiration = DateTimeOffset.UtcNow.AddSeconds(configuration.MaxSecondsToConfirmSeat);
        var seatKey = SeatKeyUtilities.GenerateKey();

        if (!_authorizationCheck.CanMakeReservation(configuration))
        {
            return Error.Failure("Reservations are not open.");
        }

        if (!await DoesSeatExist(request.SeatNumber))
        {
            return Error.NotFound();
        }

        // This is where the magic happens. Only one person can lock each seat.
        if (!await _seatLocksDatabase.LockSeat(request.SeatNumber, expiration, seatKey))
        {
            return Error.Conflict();
        }

        await UpdateSeatStatus(request.SeatNumber);

        return new LockSeatCommandResponse
        {
            SeatNumber = request.SeatNumber,
            SeatKey = seatKey,
            LockExpiration = expiration,
        };
    }

    private async Task<bool> DoesSeatExist(int seatNumber)
    {
        var seatEntity = await _seatsDatabase.FetchSeat(seatNumber);
        return seatEntity != null;
    }

    private async Task UpdateSeatStatus(int seatNumber)
    {
        var status = SeatStatus.Locked.ToString();
        var result = await _seatsDatabase.UpdateSeatStatus(seatNumber, status);
        Debug.Assert(result, "Updating the seat status should not have failed here.");
    }
}
