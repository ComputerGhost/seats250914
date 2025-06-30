using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Accounts;
internal class UpdateAccountCommandHandler(IAccountsDatabase accountsDatabase)
    : IRequestHandler<UpdateAccountCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Updating account {Login}.", request.Login);
        Log.Debug("Account data being saved is {@request}.", request);

        var accountEntity = request.ToAccountEntityModel();
        if (await accountsDatabase.UpdateAccount(accountEntity))
            return Result.Updated;

        Log.Warning("The account {Login} could not be updated because it does not exist.", request.Login);
        return Error.NotFound();
    }
}
