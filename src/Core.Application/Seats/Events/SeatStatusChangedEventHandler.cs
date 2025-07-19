using Core.Domain.Reservations;
using MediatR;

namespace Core.Application.Seats.Events;
internal class SeatStatusChangedEventHandler(ISeatChangeHandler? handler = null) : INotificationHandler<SeatStatusesChangedNotification>
{
    public async Task Handle(SeatStatusesChangedNotification notification, CancellationToken cancellationToken)
    {
        if (handler != null)
        {
            await handler.OnSeatStatusesChanged();
        }
    }
}
