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
        await UpdateSeatStatuses(reservationEntity.SeatNumbers, SeatStatus.ReservationConfirmed);

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
        await UpdateSeatStatuses(reservationEntity.SeatNumbers, SeatStatus.Available);
        await _seatLocksDatabase.DeleteLocks(reservationEntity.SeatNumbers);

        return true;
    }

    public async Task<int?> ReserveSeats(IList<int> seatNumbers, IdentityModel identity)
    {
        if (seatNumbers.Count == 0)
        {
            Log.Warning("A reservation could not be made because no seats were listed for it.");
            return null;
        }

        var reservationId = await CreateReservation(identity);
        if (reservationId == null)
        {
            return null;
        }

        if (await _seatLocksDatabase.ClearLockExpirations(seatNumbers) != seatNumbers.Count)
        {
            Log.Warning("A reservation could not be made for seats {seatNumbers} because the locks expired before processing completed.");
            await _seatLocksDatabase.DeleteLocks(seatNumbers);
            await _reservationsDatabase.DeleteReservation(reservationId.Value);
            return null;
        }

        var count = await _seatLocksDatabase.AttachLocksToReservation(seatNumbers, reservationId.Value);
        Debug.Assert(count == seatNumbers.Count, "Attaching locks to reservation should not fail here.");

        await UpdateSeatStatuses(seatNumbers, SeatStatus.AwaitingPayment);

        return reservationId;
    }

    private async Task<int?> CreateReservation(IdentityModel identity)
    {
        Debug.Assert(identity.Name != null);
        Debug.Assert(identity.Email != null);
        Debug.Assert(identity.PreferredLanguage != null);

        var entityModel = new ReservationEntityModel
        {
            ReservedAt = DateTimeOffset.UtcNow,
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

    private async Task UpdateSeatStatuses(IEnumerable<int> seatNumbers, SeatStatus status)
    {
        var result = await _seatsDatabase.UpdateSeatStatuses(seatNumbers, status.ToString());
        Debug.Assert(result == seatNumbers.Count(), $"Updating the statuses of seats {string.Join(", ", seatNumbers)} should not have failed here.");

        foreach (var seatNumber in seatNumbers)
        {
            await _mediator.Publish(new SeatStatusChangedNotification(seatNumber, status));
        }
    }
}
