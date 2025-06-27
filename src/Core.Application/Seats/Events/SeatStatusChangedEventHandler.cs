using Core.Domain.Reservations;
using MediatR;

namespace Core.Application.Seats.Events;
internal class SeatStatusChangedEventHandler(ISeatChangeHandler? handler = null) : INotificationHandler<SeatStatusChangedNotification>
{
    public async Task Handle(SeatStatusChangedNotification notification, CancellationToken cancellationToken)
    {
        if (handler != null)
        {
            await handler.OnSeatStatusChanged(notification.SeatNumber, notification.NewStatus);
        }
    }
}
