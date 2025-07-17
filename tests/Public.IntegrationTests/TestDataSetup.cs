using Core.Application.Seats;
using Core.Application.System;
using Core.Domain.Reservations;
using Core.Infrastructure;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Public.IntegrationTests;
internal class TestDataSetup
{
    public static SaveConfigurationCommand WorkingSaveConfigurationCommand => new()
    {
        ForceCloseReservations = false,
        ForceOpenReservations = true,
        MaxSeatsPerIPAddress = int.MaxValue,
        MaxSeatsPerPerson = int.MaxValue,
        MaxSeatsPerReservation = 4, // int.MaxValue will render a huge page.
        MaxSecondsToConfirmSeat = 3600,
        ScheduledCloseTimeZone = "UTC",
        ScheduledOpenTimeZone = "UTC",
    };

    private static string ConnectionString => ConfigurationAccessor.Instance.Services.GetService<IOptions<InfrastructureOptions>>()!.Value.DatabaseConnectionString;
    private static IMediator Mediator => ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;

    public static async Task DeleteAllReservations()
    {
        using (var connection = new SqlConnection(ConnectionString))
        {
            var sql = """
                DELETE FROM SeatLocks;
                DELETE FROM Reservations;
                DELETE FROM EmailQueue;
                """;
            await connection.ExecuteAsync(sql);
        }

        var seatLockService = ConfigurationAccessor.Instance.Services.GetService<ISeatLockService>()!;
        var seats = await Mediator.Send(new ListSeatsQuery());
        foreach (var seat in seats.Data)
        {
            await seatLockService.UnlockSeat(seat.SeatNumber);
        }
    }
}
