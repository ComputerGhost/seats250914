using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;

namespace Core.Application.Accounts;
internal class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, ErrorOr<Success>>
{
    private readonly IAccountsDatabase _accountsDatabase;

    public UpdateAccountCommandHandler(IAccountsDatabase accountsDatabase)
    {
        _accountsDatabase = accountsDatabase;
    }

    public async Task<ErrorOr<Success>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var accountEntity = request.ToAccountEntityModel();
        return (await _accountsDatabase.UpdateAccount(accountEntity))
            ? Result.Success : Error.NotFound();
    }
}
