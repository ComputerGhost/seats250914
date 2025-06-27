using Core.Domain.Common.Enumerations;
using MediatR;

namespace Core.Domain.Reservations;

public record SeatStatusChangedNotification(int SeatNumber, SeatStatus NewStatus) : INotification;
