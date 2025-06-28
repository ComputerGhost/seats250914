using Core.Domain.Common.Enumerations;
using MediatR;

namespace Core.Application.Seats;
public class ListSeatsQuery : IRequest<ListSeatsQueryResponse>
{
    /// <summary>
    /// Optional filter by status.
    /// </summary>
    public SeatStatus? StatusFilter { get; set; } = null;
}
