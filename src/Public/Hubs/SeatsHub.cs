using Core.Application.Seats;
using Core.Application.Seats.Events;
using Core.Domain.Common.Enumerations;
using Core.Domain.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Public.Extensions;

namespace Public.Hubs;

public class SeatsHub : Hub
{
    public const int MAX_CONNECTIONS = 300;
    public const string SEATS_UPDATED = nameof(SEATS_UPDATED);

    private static int _activeConnections = 0;
    private static readonly object _lock = new();

    public override Task OnConnectedAsync()
    {
        lock (_lock)
        {
            if (_activeConnections >= MAX_CONNECTIONS)
            {
                Context.Abort();
                return Task.CompletedTask;
            }

            ++_activeConnections;
        }

        return base.OnConnectedAsync();
    }

    [ServiceImplementation]
    private class SeatStatusChangeHandler : ISeatChangeHandler
    {
        private readonly IHubContext<SeatsHub> _hub;
        private readonly IMediator _mediator;

        public SeatStatusChangeHandler(IHubContext<SeatsHub> hub, IMediator mediator)
        {
            _hub = hub;
            _mediator = mediator;
        }

        public async Task OnSeatStatusChanged(int seatNumber, SeatStatus newStatus)
        {
            var allSeats = await _mediator.Send(new ListSeatsQuery());
            var newStatuses = allSeats.Data.ToDictionary(
                v => v.SeatNumber,
                v => v.Status.ToCssClass());
            await _hub.Clients.All.SendAsync(SEATS_UPDATED, newStatuses);
        }
    }
}
