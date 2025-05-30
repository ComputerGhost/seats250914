using Core.Domain.Common.Ports;
using MediatR;

namespace Core.Application.Accounts;
internal class ListAccountsQueryHandler : IRequestHandler<ListAccountsQuery, ListAccountsQueryResponse>
{
    private readonly IAccountsDatabase _accountsDatabase;

    public ListAccountsQueryHandler(IAccountsDatabase accountsDatabase)
    {
        _accountsDatabase = accountsDatabase;
    }

    public async Task<ListAccountsQueryResponse> Handle(ListAccountsQuery request, CancellationToken cancellationToken)
    {
        var accountEntities = await _accountsDatabase.ListAccounts();

        return new ListAccountsQueryResponse
        {
            Data = accountEntities.Select(accountEntity => new ListAccountsQueryResponseItem
            {
                IsEnabled = accountEntity.IsEnabled,
                Login = accountEntity.Login,
            }),
        };
    }
}
