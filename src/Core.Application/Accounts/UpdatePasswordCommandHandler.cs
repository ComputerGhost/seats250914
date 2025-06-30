using Core.Domain.Authentication;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Accounts;
internal class UpdatePasswordCommandHandler(IAccountsDatabase accountsDatabase)
    : IRequestHandler<UpdatePasswordCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Updating password for account {Login}", request.Login);

        var hashedPassword = new PasswordHasher().HashPassword(request.Password);
        if (await accountsDatabase.UpdatePassword(request.Login, hashedPassword))
        {
            return Result.Updated;
        }

        Log.Warning("The password for the account {Login} could not be updated because it does not exist.", request.Login);
        return Error.NotFound();
    }
}
