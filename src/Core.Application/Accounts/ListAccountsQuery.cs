using MediatR;

namespace Core.Application.Accounts;
public class ListAccountsQuery : IRequest<ListAccountsQueryResponse>
{
    // There are no parameters. All users are always returned.
}
