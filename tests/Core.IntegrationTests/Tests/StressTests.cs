using Core.Application.Configuration;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Core.IntegrationTests.Tests;

[TestClass]
public class StressTests
{
    private MinimalApplication _app = null!;

    [TestInitialize]
    public void Initialize()
    {
        _app = MinimalApplication.Create();
    }

    [TestMethod]
    public async Task Reservation_WhenManyUsers_PleaseWork()
    {
        // Arrange
        const int ACTOR_COUNT = 25;
        const double TARGET_DURATION = 1.0;
        var actors = CreateActors(ACTOR_COUNT);
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        var reservationTasks = actors.Select(a => a.ReserveSeat());
        await Task.WhenAll(reservationTasks);
        stopwatch.Stop();

        // Assert
        var elapsed = stopwatch.Elapsed.TotalSeconds;
        Assert.IsTrue(elapsed < TARGET_DURATION, $"Reservations took {elapsed}s. The target is {TARGET_DURATION}s.");
    }

    private IEnumerable<Actor> CreateActors(int count)
    {
        for (int i = 0; i != count; ++i)
        {
            yield return new Actor(_app);
        }
    }

    private class Actor
    {
        private readonly IMediator _mediator;

        public Actor(MinimalApplication app)
        {
            _mediator = app.ServiceProvider.GetRequiredService<IMediator>();
        }

        public async Task ReserveSeat()
        {
            // I'll replace this with the actual calls later.
            await _mediator.Send(new TestDatabaseQuery()); // Get available
            await _mediator.Send(new TestDatabaseQuery()); // Attempt reservation
            await _mediator.Send(new TestDatabaseQuery()); // Another got it, try again.
        }
    }
}
