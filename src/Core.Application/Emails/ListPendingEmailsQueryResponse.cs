namespace Core.Application.Emails;
public class ListPendingEmailsQueryResponse
{
    public required IEnumerable<ListPendingEmailsQueryResponseItem> Data { get; init; } = null!;
}
