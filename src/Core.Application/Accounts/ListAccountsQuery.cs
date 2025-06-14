using MediatR;

namespace Core.Application.Accounts;
public class ListAccountsQuery : IRequest<ListAccountsQueryResponse>
{
    // There are no parameters. All accounts are always returned.
}
