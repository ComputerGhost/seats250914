using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Core.Infrastructure.Database;

[ServiceImplementation]
internal class ReservationsDatabase(IDbConnection connection) : IReservationsDatabase
{
    public async Task<int> CountActiveReservationsForEmailAddress(string emailAddress)
    {
        var sql = """
            SELECT COUNT(*)
            FROM Reservations
            LEFT JOIN ReservationStatuses ON ReservationStatuses.Id = Reservations.ReservationStatusId
            WHERE Email = @emailAddress
            AND ReservationStatuses.Status IN ('AwaitingPayment', 'ReservationConfirmed')
            """;
        return await connection.ExecuteScalarAsync<int>(sql, new { emailAddress });
    }

    public async Task<int?> CreateReservation(ReservationEntityModel reservation)
    {
        // This assumes that the seat number and status exist.
        var sql = """
            INSERT INTO Reservations (ReservationStatusId, SeatId, SeatLockId, ReservedAt, Name, Email, PhoneNumber, PreferredLanguage)
            OUTPUT INSERTED.Id
            SELECT
                ReservationStatuses.Id,
                Seats.Id,
                SeatLocks.Id,
                @reservedAt, @name, @email, @phoneNumber, @preferredLanguage
            FROM Seats
            LEFT JOIN SeatLocks ON SeatLocks.SeatId = Seats.Id
            LEFT JOIN ReservationStatuses ON ReservationStatuses.Status = @status
            WHERE Seats.Number = @seatNumber
            """;

        try
        {
            return await connection.ExecuteScalarAsync<int>(sql, reservation);
        }
        // Catch unique constraint violations.
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return null;
        }
    }

    public async Task DeleteReservation(int reservationId)
    {
        var sql = "DELETE FROM Reservations WHERE Id = @reservationId";
        await connection.ExecuteAsync(sql, new { reservationId });
    }

    public async Task<ReservationEntityModel?> FetchReservation(int reservationId)
    {
        ReservationEntityModel? reservation = null;

        var sql = """
            SELECT Reservations.*, ReservationStatuses.Status, Seats.Number SeatNumber
            FROM Reservations
            LEFT JOIN SeatLocks ON SeatLocks.ReservationId = Reservations.Id
            LEFT JOIN Seats ON Seats.Id = SeatLocks.SeatId
            LEFT JOIN ReservationStatuses ON ReservationStatuses.Id = Reservations.ReservationStatusId
            WHERE Reservations.Id = @reservationId
            """;
        await connection.QueryAsync<ReservationEntityModel, int, ReservationEntityModel>(
            sql,
            (res, seatNumber) =>
            {
                reservation ??= res;
                reservation.SeatNumbers.Add(seatNumber);
                return res;
            },
            new { reservationId },
            splitOn: "SeatNumber"
        );

        return reservation;
    }

    public async Task<IEnumerable<ReservationEntityModel>> ListReservations()
    {
        var reservations = new Dictionary<int, ReservationEntityModel>();

        var sql = """
            SELECT Reservations.*, ReservationStatuses.Status, Seats.Number SeatNumber
            FROM Reservations
            LEFT JOIN SeatLocks ON SeatLocks.ReservationId = Reservations.Id
            LEFT JOIN Seats ON Seats.Id = SeatLocks.SeatId
            LEFT JOIN ReservationStatuses ON ReservationStatuses.Id = Reservations.ReservationStatusId
            ORDER BY Reservations.ReservedAt ASC
            """;

        await connection.QueryAsync<ReservationEntityModel, int, ReservationEntityModel>(
            sql,
            (reservation, seatNumber) =>
            {
                if (!reservations.TryGetValue(reservation.Id, out var entry))
                {
                    entry = reservation;
                    reservations.Add(entry.Id, entry);
                }

                entry.SeatNumbers.Add(seatNumber);
                return entry;
            },
            splitOn: "SeatNumber"
        );

        return reservations.Values;
    }

    public async Task<bool> UpdateReservation(ReservationEntityModel reservation)
    {
        var sql = """
            UPDATE Reservations SET
                Name = @name,
                Email = @email,
                PhoneNumber = @phoneNumber,
                PreferredLanguage = @preferredLanguage
                -- These columns aren't updated:
                --  * ReservationStatusId
                --  * SeatId
                --  * SeatLockId
                --  * ReservedAt
            WHERE Id = @id
            """;
        return await connection.ExecuteAsync(sql, reservation) > 0;
    }

    public async Task<bool> UpdateReservationStatus(int reservationId, string reservationStatus)
    {
        var sql = """
            UPDATE Reservations 
            SET ReservationStatusId = (SELECT Id FROM ReservationStatuses WHERE Status = @reservationStatus)
            WHERE Id = @reservationId
            """;
        return await connection.ExecuteAsync(sql, new
        {
            reservationId,
            reservationStatus,
        }) > 0;
    }
}
