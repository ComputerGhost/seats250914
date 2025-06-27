using Core.Application.Seats;
using Core.Application.Seats.Events;
using Core.Domain.Common.Enumerations;
using Core.Domain.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Public.Hubs;

public class SeatsHub : Hub
{
    public const string SEATS_UPDATED = nameof(SEATS_UPDATED);

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
                v => StatusEnumToString(v.Status));
            await _hub.Clients.All.SendAsync(SEATS_UPDATED, newStatuses);
        }

        private static string StatusEnumToString(SeatStatus statusEnum)
        {
            return statusEnum switch
            {
                SeatStatus.Available => "available",
                SeatStatus.Locked => "on-hold",
                SeatStatus.AwaitingPayment => "on-hold",
                SeatStatus.ReservationConfirmed => "reserved",
                _ => throw new NotImplementedException()
            };
        }
    }
}
