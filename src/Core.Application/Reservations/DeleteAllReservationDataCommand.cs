using MediatR;

namespace Core.Application.Reservations;

/// <summary>
/// Deletes all reservation data and seat locks.
/// </summary>
/// <remarks>
/// This is included for integration tests on localhost.
/// Please do not use this outside of tests on localhost.
/// </remarks>
public class DeleteAllReservationDataCommand : IRequest
{
}
