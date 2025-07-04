using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Dapper;
using System.Data;

namespace Core.Infrastructure.Adapters;

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

    public async Task<int> CreateReservation(ReservationEntityModel reservation)
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
        return await connection.ExecuteScalarAsync<int>(sql, reservation);
    }

    public async Task<int> DeleteAllReservations()
    {
        var sql = "DELETE FROM Reservations";
        return await connection.ExecuteAsync(sql);
    }

    public async Task<ReservationEntityModel?> FetchReservation(int reservationId)
    {
        var sql = """
            SELECT Reservations.*, Seats.Number SeatNumber, ReservationStatuses.Status
            FROM Reservations
            LEFT JOIN Seats ON Seats.Id = Reservations.SeatId
            LEFT JOIN ReservationStatuses ON ReservationStatuses.Id = Reservations.ReservationStatusId
            WHERE Reservations.Id = @reservationId
            """;
        return await connection.QuerySingleOrDefaultAsync<ReservationEntityModel>(sql, new
        {
            reservationId,
        });
    }

    public async Task<IEnumerable<ReservationEntityModel>> ListReservations()
    {
        var sql = """
            SELECT Reservations.*, Seats.Number SeatNumber, ReservationStatuses.Status
            FROM Reservations
            LEFT JOIN Seats ON Seats.Id = Reservations.SeatId
            LEFT JOIN ReservationStatuses ON ReservationStatuses.Id = Reservations.ReservationStatusId
            ORDER BY Reservations.ReservedAt ASC
            """;
        return await connection.QueryAsync<ReservationEntityModel>(sql);
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
