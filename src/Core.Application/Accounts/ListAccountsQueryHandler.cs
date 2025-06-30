using Core.Domain.Common.Ports;
using MediatR;
using Serilog;

namespace Core.Application.Accounts;
internal class ListAccountsQueryHandler(IAccountsDatabase accountsDatabase) : IRequestHandler<ListAccountsQuery, ListAccountsQueryResponse>
{
    public async Task<ListAccountsQueryResponse> Handle(ListAccountsQuery request, CancellationToken cancellationToken)
    {
        Log.Information("Listing all accounts.");

        var accountEntities = await accountsDatabase.ListAccounts();

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
