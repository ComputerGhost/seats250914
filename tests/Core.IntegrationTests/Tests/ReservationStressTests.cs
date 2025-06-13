using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Core.IntegrationTests.Tests;

[Ignore("This is ignored because it modifies the database.")]
[TestClass]
public class ReservationStressTests
{
    const int ACTOR_COUNT = 25;
    const double MAX_DURATION = 1.0;
    const int SEAT_COUNT = 100;

    private IMediator _mediator = null!;

    [TestInitialize]
    public void Initialize()
    {
        var app = MinimalApplication.Create();
        _mediator = app.ServiceProvider.GetRequiredService<IMediator>();
    }

    [TestMethod]
    public async Task Reservation_WhenManyUsers_PleaseWork()
    {
        // Arrange
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

    private class Actor
    {
        private readonly IMediator _mediator;

        public Actor(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task ReserveSeat()
        {
            var seatLock = await SelectSeat();
            if (seatLock == null)
            {
                // No seats available
                return;
            }

            await _mediator.Send(new ReserveSeatCommand
            {
                Email = $"Email{seatLock.SeatNumber}@test.com",
                Name = $"Name {seatLock.SeatNumber}",
                PreferredLanguage = "English",
                SeatKey = seatLock.SeatKey,
                SeatNumber = seatLock.SeatNumber,
            });
        }

        private async Task<LockSeatCommandResponse?> SelectSeat()
        {
            for (int i = 1; i <= SEAT_COUNT; ++i)
            {
                var result = await _mediator.Send(new LockSeatCommand { SeatNumber = i });
                if (!result.IsError)
                {
                    return result.Value;
                }
            }

            return null;
        }
    }
}
