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
internal class ReservationService(IMediator mediator, IUnitOfWork unitOfWork) : IReservationService
{
    public async Task<bool> ApproveReservation(int reservationId)
    {
        return await unitOfWork.RunInTransaction(async () =>
        {
            var reservationEntity = await unitOfWork.Reservations.FetchReservation(reservationId);
            if (reservationEntity == null)
            {
                return false;
            }

            await UpdateReservationStatus(reservationId, ReservationStatus.ReservationConfirmed);
            await UpdateSeatStatuses(reservationEntity.SeatNumbers, SeatStatus.ReservationConfirmed);

            await mediator.Publish(new SeatStatusesChangedNotification());

            return true;
        });
    }

    public async Task<bool> RejectReservation(int reservationId)
    {
        return await unitOfWork.RunInTransaction(async () =>
        {
            var reservationEntity = await unitOfWork.Reservations.FetchReservation(reservationId);
            if (reservationEntity == null)
            {
                return false;
            }

            await UpdateReservationStatus(reservationId, ReservationStatus.ReservationRejected);
            await UpdateSeatStatuses(reservationEntity.SeatNumbers, SeatStatus.Available);
            await unitOfWork.SeatLocks.DeleteLocks(reservationEntity.SeatNumbers);

            await mediator.Publish(new SeatStatusesChangedNotification());

            return true;
        });
    }

    public async Task<int?> ReserveSeats(IList<int> seatNumbers, IdentityModel identity)
    {
        if (seatNumbers.Count == 0)
        {
            Log.Warning("A reservation could not be made because no seats were listed for it.");
            return null;
        }

        return await unitOfWork.RunInTransaction(async () =>
        {
            var reservationId = await CreateReservation(identity);
            if (reservationId == null)
            {
                return null;
            }

            if (await unitOfWork.SeatLocks.ClearLockExpirations(seatNumbers) != seatNumbers.Count)
            {
                Log.Warning("A reservation could not be made for seats {seatNumbers} because the locks expired before processing completed.");
                unitOfWork.Rollback();
                return null;
            }

            await AttachLocksToReservation(seatNumbers, reservationId.Value);
            await AttachSeatsToReservation(seatNumbers, reservationId.Value);
            await UpdateSeatStatuses(seatNumbers, SeatStatus.AwaitingPayment);

            await mediator.Publish(new SeatStatusesChangedNotification());

            return reservationId;
        });
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

        var result = await unitOfWork.Reservations.CreateReservation(entityModel);
        if (result == null)
        {
            Log.Error("Reservation creation failed for seat {seatNumber}. It's likely that the seat key is already used.");
        }

        return result;
    }

    private async Task AttachLocksToReservation(IList<int> seatNumbers, int reservationId)
    {
        var count = await unitOfWork.SeatLocks.AttachLocksToReservation(seatNumbers, reservationId);
        Debug.Assert(count == seatNumbers.Count, "Attaching locks to reservation should not fail here.");
    }

    private async Task AttachSeatsToReservation(IList<int> seatNumbers, int reservationId)
    {
        var count = await unitOfWork.Seats.AttachSeatsToReservation(seatNumbers, reservationId);
        Debug.Assert(count == seatNumbers.Count, "Attaching seats to reservation should not fail here.");
    }

    private async Task UpdateReservationStatus(int reservationId, ReservationStatus status)
    {
        var result = await unitOfWork.Reservations.UpdateReservationStatus(reservationId, status.ToString());
        Debug.Assert(result, $"Updating the status of reservation {reservationId} should not have failed here");
    }

    private async Task UpdateSeatStatuses(IList<int> seatNumbers, SeatStatus status)
    {
        var count = await unitOfWork.Seats.UpdateSeatStatuses(seatNumbers, status.ToString());
        Debug.Assert(count == seatNumbers.Count, $"Updating the statuses of seats {string.Join(", ", seatNumbers)} should not have failed here.");
    }
}
