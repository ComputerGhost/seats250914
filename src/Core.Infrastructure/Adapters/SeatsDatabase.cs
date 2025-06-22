using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Dapper;
using System.Data;

namespace Core.Infrastructure.Adapters;

[ServiceImplementation]
internal class SeatsDatabase(IDbConnection connection) : ISeatsDatabase
{
    public async Task<SeatEntityModel?> FetchSeat(int seatNumber)
    {
        var sql = """
            SELECT Seats.Number, SeatStatuses.Status
            FROM Seats
            LEFT JOIN SeatStatuses ON SeatStatuses.Id = Seats.SeatStatusId
            WHERE Seats.Number = @seatNumber
            """;
        return await connection.QuerySingleOrDefaultAsync<SeatEntityModel>(sql, new
        {
            seatNumber,
        });
    }

    public async Task<IEnumerable<SeatEntityModel>> ListSeats()
    {
        var sql = """
            SELECT Seats.Number, SeatStatuses.Status
            FROM Seats
            LEFT JOIN SeatStatuses ON SeatStatuses.Id = Seats.SeatStatusId
            ORDER BY Seats.Number
            """;
        return await connection.QueryAsync<SeatEntityModel>(sql);
    }

    public async Task ResetUnlockedSeatStatuses()
    {
        var sql = """
            UPDATE s
            SET SeatStatusId = (SELECT Id FROM SeatStatuses WHERE Status = 'Available')
            FROM Seats s
            LEFT JOIN SeatStatuses ss ON ss.Id = s.SeatStatusId
            LEFT JOIN SeatLocks sl ON sl.SeatId = s.Id
            WHERE ss.Status = 'Locked' AND sl.Id IS NULL
            """;
        await connection.ExecuteAsync(sql);
    }

    public async Task<bool> UpdateSeatStatus(int seatNumber, string seatStatus)
    {
        var sql = """
            UPDATE Seats
            SET SeatStatusId = (SELECT Id FROM SeatStatuses WHERE Status = @seatStatus)
            WHERE Seats.Number = @seatNumber
            """;
        return await connection.ExecuteAsync(sql, new
        {
            seatNumber,
            seatStatus,
        }) > 0;
    }
}
