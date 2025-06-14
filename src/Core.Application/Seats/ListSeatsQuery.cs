using MediatR;

namespace Core.Application.Seats;
public class ListSeatsQuery : IRequest<ListSeatsQueryResponse>
{
    // There are no parameters. All seats are always returned.
}
