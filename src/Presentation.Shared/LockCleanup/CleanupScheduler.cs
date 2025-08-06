using Core.Application.System;
using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Presentation.Shared.LockCleanup;
public class CleanupScheduler : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly PriorityQueue<DateTimeOffset, DateTimeOffset> _schedule = new();
    private readonly Lock _scheduleLock = new();
    private readonly SemaphoreSlim _signal = new(0);

    public CleanupScheduler(IServiceProvider services)
    {
        _services = services;
    }

    public required int MaxWaitSeconds { get; set; }

    /// <summary>
    /// A short delay to ensure the expiration has definitely passed by cleanup time.
    /// </summary>
    public int ProcessingDelaySeconds { get; set; } = 1;

    public async Task ScheduleCleanup()
    {
        var secondsToExpire = await GetLockExpirationSeconds() + ProcessingDelaySeconds;
        var when = DateTimeOffset.UtcNow.AddSeconds(secondsToExpire);

        Log.Information("Scheduling a cleanup for {secondsToExpire} seconds from now.", secondsToExpire);

        lock (_scheduleLock)
        {
            _schedule.Enqueue(when, when);
            _signal.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CleanExpiredLocks(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var nextSchedule = GetNextSchedule();
            var waitTime = nextSchedule - DateTimeOffset.UtcNow;
            await _signal.WaitAsync(waitTime, stoppingToken);

            // Adding a new schedule will get here too,
            // so check if we're here because it's time.
            if (nextSchedule <= DateTimeOffset.UtcNow)
            {
                await CleanExpiredLocks(stoppingToken);
            }
            else
            {
                lock (_scheduleLock)
                {
                    _schedule.Enqueue(nextSchedule, nextSchedule);
                }
            }
        }
    }

    private async Task CleanExpiredLocks(CancellationToken stoppingToken)
    {
        Log.Information("Cleaning up expired locks.");

        using var scope = _services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new ClearExpiredLocksCommand(), stoppingToken);
    }

    private async Task<int> GetLockExpirationSeconds()
    {
        using var scope = _services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var result = await mediator.Send(new FetchConfigurationQuery());
        return result.MaxSecondsToConfirmSeat + result.GracePeriodSeconds;
    }

    private DateTimeOffset GetNextSchedule()
    {
        lock (_scheduleLock)
        {
            if (_schedule.TryPeek(out var when, out _))
            {
                _schedule.Dequeue();
                return when;
            }
        }

        return DateTimeOffset.UtcNow + TimeSpan.FromSeconds(MaxWaitSeconds);
    }
}
