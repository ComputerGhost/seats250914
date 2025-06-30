using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Accounts;
internal class FetchAccountQueryHandler(IAccountsDatabase accountsDatabase) : IRequestHandler<FetchAccountQuery, ErrorOr<FetchAccountQueryResponse>>
{
    public async Task<ErrorOr<FetchAccountQueryResponse>> Handle(FetchAccountQuery request, CancellationToken cancellationToken)
    {
        Log.Information("Fetching account information for {Login}", request.Login);

        var accountEntity = await accountsDatabase.FetchAccount(request.Login);
        if (accountEntity == null)
        {
            Log.Information("Account {Login} was not found.", request.Login);
            return Error.NotFound();
        }

        return new FetchAccountQueryResponse
        {
            IsEnabled = accountEntity.IsEnabled,
            Login = accountEntity.Login,
        };
    }
}
