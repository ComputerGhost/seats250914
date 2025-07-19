using MediatR;

namespace Core.Domain.Reservations;

/// <summary>
/// At least one seat statuses was updated.
/// </summary>
/// <remarks>
/// We don't care about individual updates because we pull them all at once anyways.
/// </remarks>
public record SeatStatusesChangedNotification() : INotification;
