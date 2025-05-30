namespace Core.Application.Accounts;
public class ListAccountsQueryResponse
{
    public required IEnumerable<ListAccountsQueryResponseItem> Data { get; init; } = null!;
}
