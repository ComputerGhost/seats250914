using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Models;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using MediatR;
using Serilog;
using System.Diagnostics;

namespace Core.Domain.Reservations;

[ServiceImplementation]
internal class ReservationService : IReservationService
{
    private readonly IMediator _mediator;
    private readonly IReservationsDatabase _reservationsDatabase;
    private readonly ISeatLocksDatabase _seatLocksDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public ReservationService(
        IMediator mediator,
        IReservationsDatabase reservationsDatabase,
        ISeatLocksDatabase seatLocksDatabase,
        ISeatsDatabase seatsDatabase)
    {
        _mediator = mediator;
        _reservationsDatabase = reservationsDatabase;
        _seatLocksDatabase = seatLocksDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task<bool> ApproveReservation(int reservationId)
    {
        var reservationEntity = await _reservationsDatabase.FetchReservation(reservationId);
        if (reservationEntity == null)
        {
            return false;
        }

        await UpdateReservationStatus(reservationId, ReservationStatus.ReservationConfirmed);
        await UpdateSeatStatus(reservationEntity.SeatNumber, SeatStatus.ReservationConfirmed);

        return true;
    }

    public async Task<bool> RejectReservation(int reservationId)
    {
        var reservationEntity = await _reservationsDatabase.FetchReservation(reservationId);
        if (reservationEntity == null)
        {
            return false;
        }

        await UpdateReservationStatus(reservationId, ReservationStatus.ReservationRejected);
        await UpdateSeatStatus(reservationEntity.SeatNumber, SeatStatus.Available);
        await _seatLocksDatabase.DeleteLock(reservationEntity.SeatNumber);

        return true;
    }

    public async Task<int?> ReserveSeat(int seatNumber, IdentityModel identity)
    {
        await _seatLocksDatabase.ClearLockExpiration(seatNumber);

        // Race condition check -- make sure the lock didn't just expire.
        if (await _seatLocksDatabase.FetchSeatLock(seatNumber) == null)
        {
            return null;
        }

        var reservationId = await CreateReservation(seatNumber, identity);

        if (reservationId != null)
        {
            await UpdateSeatStatus(seatNumber, SeatStatus.AwaitingPayment);
        }

        return reservationId;
    }

    public async Task<int?> ReserveSeats(IList<int> seatNumbers, IdentityModel identity)
    {
        // Note the first seat for backwards compatibility.
        // This can be deleted nearer the end of the sprint.
        var compatSeat = seatNumbers.First();

        var reservationId = await CreateReservation(compatSeat, identity);
        if (reservationId == null)
        {
            return null;
        }

        if (await _seatLocksDatabase.ClearLockExpirations(seatNumbers) != seatNumbers.Count)
        {
            Log.Warning("A reservation could not be made for seats {seatNumbers} because the locks expired before processing completed.");
            await _seatLocksDatabase.DeleteLocks(seatNumbers);
            return null;
        }

        await _reservationsDatabase.AttachSeatsToReservation(reservationId.Value, seatNumbers);

        foreach (var seatNumber in seatNumbers)
        {
            await UpdateSeatStatus(seatNumber, SeatStatus.AwaitingPayment);
        }

        return reservationId;
    }

    private async Task<int?> CreateReservation(int seatNumber, IdentityModel identity)
    {
        Debug.Assert(identity.Name != null);
        Debug.Assert(identity.Email != null);
        Debug.Assert(identity.PreferredLanguage != null);

        var entityModel = new ReservationEntityModel
        {
            ReservedAt = DateTimeOffset.UtcNow,
            SeatNumber = seatNumber,
            Name = identity.Name,
            Email = identity.Email,
            PhoneNumber = identity.PhoneNumber,
            PreferredLanguage = identity.PreferredLanguage,
            Status = ReservationStatus.AwaitingPayment.ToString(),
        };

        var result = await _reservationsDatabase.CreateReservation(entityModel);
        if (result == null)
        {
            Log.Error("Reservation creation failed for seat {seatNumber}. It's likely that the seat key is already used.");
        }

        return result;
    }

    private async Task UpdateReservationStatus(int reservationId, ReservationStatus status)
    {
        var result = await _reservationsDatabase.UpdateReservationStatus(reservationId, status.ToString());
        Debug.Assert(result, $"Updating the status of reservation {reservationId} should not have failed here");
    }

    private async Task UpdateSeatStatus(int seatNumber, SeatStatus status)
    {
        var result = await _seatsDatabase.UpdateSeatStatus(seatNumber, status.ToString());
        Debug.Assert(result, $"Updating the status of seat {seatNumber} should not have failed here.");

        await _mediator.Publish(new SeatStatusChangedNotification(seatNumber, status));
    }
}
