using Core.Application.Seats.Enumerations;
using Core.Domain.Authorization;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using System.Diagnostics;

namespace Core.Application.Reservations;
public class ReserveSeatCommandHandler : IRequestHandler<ReserveSeatCommand, ErrorOr<Success>>
{
    private readonly IReservationAuthorizationChecker _authorizationCheck;
    private readonly IReservationsDatabase _reservationsDatabase;
    private readonly ISeatLocksDatabase _seatLocksDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public ReserveSeatCommandHandler(
        IReservationAuthorizationChecker authorizationCheck,
        IReservationsDatabase reservationsDatabase,
        ISeatLocksDatabase seatLocksDatabase,
        ISeatsDatabase seatsDatabase)
    {
        _authorizationCheck = authorizationCheck;
        _reservationsDatabase = reservationsDatabase;
        _seatLocksDatabase = seatLocksDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task<ErrorOr<Success>> Handle(ReserveSeatCommand request, CancellationToken cancellationToken)
    {
        var unauthorizedMessage = $"User is not authorized to reserve seat {request.SeatNumber}.";

        if (!await _authorizationCheck.CanReserveSeat(request.SeatNumber, request.SeatKey))
        {
            return Error.Failure(unauthorizedMessage);
        }

        // maybe add a sql scope here???

        await _seatLocksDatabase.ClearLockExpiration(request.SeatNumber);

        // Race condition check -- make sure the lock still exists.
        if (!await DoesLockStillExist(request.SeatNumber))
        {
            return Error.Failure(unauthorizedMessage);
        }

        await CreateReservation(request);

        await UpdateSeatStatus(request.SeatNumber);

        return Result.Success;
    }

    private Task CreateReservation(ReserveSeatCommand request)
    {
        return _reservationsDatabase.CreateReservation(new ReservationEntityModel
        {
            ReservedAt = DateTimeOffset.UtcNow,
            SeatNumber = request.SeatNumber,
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = request.PreferredLanguage,
        });
    }

    private async Task<bool> DoesLockStillExist(int seatNumber)
    {
        var lockEntity = await _seatLocksDatabase.FetchSeatLock(seatNumber);
        return lockEntity != null;
    }

    private async Task UpdateSeatStatus(int seatNumber)
    {
        var status = SeatStatus.AwaitingPayment.ToString();
        var result = await _seatsDatabase.UpdateSeatStatus(seatNumber, status);
        Debug.Assert(result, "Updating the seat status should not have failed here.");
    }
}
