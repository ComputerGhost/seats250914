using Core.Application.System;
using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Diagnostics;

namespace Core.IntegrationTests.Tests;

// I wonder if I should move this to smoke tests.
// It seems a little much for an integration test, doesn't it?
// I'll consider that later.
[Ignore("This is ignored because it modifies the database.")]
[TestClass]
public class ReservationStressTests
{
    const int SEAT_COUNT = 100;

    private IMediator _mediator = null!;

    [TestInitialize]
    public async Task Initialize()
    {
        var app = MinimalApplication.Create();
        _mediator = app.ServiceProvider.GetRequiredService<IMediator>();

        await _mediator.Send(new SaveConfigurationCommand
        {
            MaxSecondsToConfirmSeat = 60 * 10,
            ForceOpenReservations = true,
            MaxSeatsPerIPAddress = int.MaxValue,
            MaxSeatsPerPerson = int.MaxValue,
            ScheduledOpenTimeZone = "UTC",
        });

        var reservations = await _mediator.Send(new ListReservationsQuery());
        foreach (var reservation in reservations.Data)
        {
            await _mediator.Send(new RejectReservationCommand(reservation.ReservationId));
        }

        await _mediator.Send(new ClearExpiredLocksCommand());

        // Because open/close doesn't like multithreading.
        app.ServiceProvider.GetRequiredService<IDbConnection>().Open();
    }

    [TestMethod]
    public async Task Reservation_WhenManyUsers_PleaseWork()
    {
        // Arrange
        const int ACTOR_COUNT = 1000;
        const double MAX_DURATION = 1.0;
        var actors = CreateActors(ACTOR_COUNT);
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        var reservationTasks = actors.Select(a => a.ReserveSeat());
        await Task.WhenAll(reservationTasks);
        stopwatch.Stop();

        // Assert
        var elapsed = stopwatch.Elapsed.TotalSeconds;
        Assert.IsTrue(elapsed < MAX_DURATION, $"Reservations by {ACTOR_COUNT} actors took {elapsed}s, but the target is {MAX_DURATION}s.");
    }

    private IEnumerable<Actor> CreateActors(int count)
    {
        for (int i = 0; i != count; ++i)
        {
            yield return new Actor(_mediator);
        }
    }

    private class Actor(IMediator mediator)
    {
        public async Task ReserveSeat()
        {
            var seatLock = await SelectSeat();
            if (seatLock == null) // No seats available
            {
                return;
            }

            if (!await ReserveSeat(seatLock.SeatNumber, seatLock.SeatKey))
            {
                throw new Exception("Failed to reserve seat.");
            }
        }

        private async Task<bool> ReserveSeat(int seatNumber, string seatKey)
        {
            var result = await mediator.Send(new ReserveSeatCommand
            {
                Email = $"Email{seatNumber}@test.com",
                IsStaff = true,
                Name = $"Name {seatNumber}",
                PreferredLanguage = "English",
                SeatKey = seatKey,
                SeatNumber = seatNumber,
            });
            return !result.IsError;
        }

        private async Task<LockSeatCommandResponse?> SelectSeat()
        {
            for (int i = 1; i <= SEAT_COUNT; ++i)
            {
                var result = await mediator.Send(new LockSeatCommand
                {
                    IpAddress = "127.0.0.1",
                    IsStaff = true,
                    SeatNumber = i,
                });
                if (!result.IsError)
                {
                    return result.Value;
                }
            }

            return null;
        }
    }
}
