namespace Core.Application.Reservations;
public class ListReservationsQueryResponse
{
    public required IEnumerable<ListReservationsQueryResponseItem> Data { get; init; } = null!;
}
