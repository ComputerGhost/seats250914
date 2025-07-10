using MediatR;

namespace Core.Application.Emails;
public class ListPendingEmailsQuery : IRequest<ListPendingEmailsQueryResponse>
{
    /// <summary>
    /// Filters by max attempts made.
    /// </summary>
    public int MaxAttempts { get; set; }
}
