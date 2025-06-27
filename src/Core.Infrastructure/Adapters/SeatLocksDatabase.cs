using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Core.Infrastructure.Adapters;

[ServiceImplementation]
internal class SeatLocksDatabase(IDbConnection connection) : ISeatLocksDatabase
{
    public async Task ClearLockExpiration(int seatNumber)
    {
        var sql = """
            UPDATE SeatLocks
            SET Expiration = NULL
            WHERE SeatId = (SELECT Id FROM Seats WHERE Number = @seatNumber)
            """;
        await connection.ExecuteAsync(sql, new
        {
            seatNumber,
        });
    }

    public async Task<int> CountLocksForIpAddress(string ipAddress)
    {
        var sql = "SELECT COUNT(*) FROM SeatLocks WHERE IpAddress = @ipAddress";
        return await connection.QuerySingleAsync<int>(sql, new
        {
            ipAddress,
        });
    }

    public async Task<bool> DeleteLock(int seatNumber)
    {
        var sql = """
            DELETE FROM SeatLocks
            WHERE SeatId = (SELECT Id FROM Seats WHERE Number = @seatNumber)
            """;
        return await connection.ExecuteAsync(sql, new
        {
            seatNumber,
        }) > 0;
    }

    public async Task<IEnumerable<SeatLockEntityModel>> FetchExpiredLocks(DateTimeOffset beforeTime)
    {
        var sql = """
            SELECT SeatLocks.*, Seats.Number
            FROM SeatLocks
            INNER JOIN Seats ON Seats.Id = SeatLocks.SeatId
            WHERE Expiration < @beforeTime
            """;
        return await connection.QueryAsync<SeatLockEntityModel>(sql, new
        {
            beforeTime,
        });
    }

    public async Task<SeatLockEntityModel?> FetchSeatLock(int seatNumber)
    {
        var sql = """
            SELECT SeatLocks.*, Seats.Number
            FROM SeatLocks
            INNER JOIN Seats ON Seats.Id = SeatLocks.SeatId
            WHERE Seats.Number = @seatNumber
            """;
        return await connection.QuerySingleOrDefaultAsync<SeatLockEntityModel>(sql, new
        {
            seatNumber,
        });
    }

    public async Task<bool> LockSeat(SeatLockEntityModel seatLockEntity)
    {
        try
        {
            var sql = """
                INSERT INTO SeatLocks (SeatId, IpAddress, [Key], LockedAt, Expiration)
                SELECT
                    Seats.Id SeatId,
                    @ipAddress, @key, @lockedAt, @expiration
                FROM Seats
                WHERE Number = @seatNumber
                """;
            return await connection.ExecuteAsync(sql, seatLockEntity) > 0;
        }
        // Catch unique constraint violations.
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return false;
        }
    }
}
