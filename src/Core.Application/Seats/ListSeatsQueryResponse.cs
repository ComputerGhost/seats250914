namespace Core.Application.Seats;
public class ListSeatsQueryResponse
{
    public required IEnumerable<ListSeatsQueryResponseItem> Data { get; init; } = null!;
}
