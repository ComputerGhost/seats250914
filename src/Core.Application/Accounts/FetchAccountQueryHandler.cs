using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;

namespace Core.Application.Accounts;
internal class FetchAccountQueryHandler : IRequestHandler<FetchAccountQuery, ErrorOr<FetchAccountQueryResponse>>
{
    private readonly IAccountsDatabase _accountsDatabase;

    public FetchAccountQueryHandler(IAccountsDatabase accountsDatabase)
    {
        _accountsDatabase = accountsDatabase;
    }

    public async Task<ErrorOr<FetchAccountQueryResponse>> Handle(FetchAccountQuery request, CancellationToken cancellationToken)
    {
        var accountEntity = await _accountsDatabase.FetchAccount(request.Login);
        if (accountEntity == null)
        {
            return Error.NotFound();
        }

        return new FetchAccountQueryResponse
        {
            IsEnabled = accountEntity.IsEnabled,
            Login = accountEntity.Login,
        };
    }
}
