using Core.Domain.Common.Models;
using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;

/// <summary>
/// Attempts to lock multiple seats.
/// </summary>
/// <remarks>
/// If two people simultaneously reserve a seat, only one will be successful.
/// An error result will be returned for the losing person.
/// <br /><br />
/// If `Error.Unauthorized` is returned, then the metadata will have an 
/// `AuthorizationResult` under the "details" key.
/// <br /><br />
/// If `Error.Conflict` is returned, then the metadata will have the 
/// conflicting seat number under the "seatNumber" key.
/// </remarks>
public class LockSeatsCommand : IRequest<ErrorOr<LockSeatsCommandResponse>>
{
    /// <summary>
    /// Ip address of the one reserving the seats.
    /// </summary>
    public required string IpAddress { get; set; } = null!;

    /// <summary>
    /// The numbers of the seats to be locked.
    /// </summary>
    public required IEnumerable<int> SeatNumbers { get; set; } = null!;

    internal IdentityModel Identity => new()
    {
        IpAddress = IpAddress,
        IsStaff = false,
    };
}
