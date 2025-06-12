using Core.Application.Configuration;
using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Core.IntegrationTests.Tests;

[TestClass]
public class ReservationStressTests
{
    const int ACTOR_COUNT = 25;
    const double MAX_DURATION = 1.0;
    const int SEAT_COUNT = 100;

    private IMediator _mediator = null!;

    [TestInitialize]
    public async Task Initialize()
    {
        var app = MinimalApplication.Create();
        _mediator = app.ServiceProvider.GetRequiredService<IMediator>();
        await RejectAllReservations();
    }

    [TestCleanup]
    public async Task CleanUp()
    {
        await RejectAllReservations();
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

    private Task RejectAllReservations()
    {
        // TODO
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
            var result = await _mediator.Send(new LockSeatCommand { SeatNumber = 1 });

            // I'll replace this with the actual calls later.
            await _mediator.Send(new TestDatabaseQuery()); // Get available
            await _mediator.Send(new TestDatabaseQuery()); // Attempt reservation
            await _mediator.Send(new TestDatabaseQuery()); // Another got it, try again.
        }
    }
}
