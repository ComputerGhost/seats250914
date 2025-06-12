using MediatR;

namespace Core.Application.Reservations;
public class ListReservationsQuery : IRequest<ListReservationsQueryResponse>
{
    // There are no parameters. All reservations are always returned.
}
