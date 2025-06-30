using Core.Domain.Authentication;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Accounts;
internal class VerifyPasswordCommandHandler(IAccountsDatabase accountsDatabase)
    : IRequestHandler<VerifyPasswordCommand, ErrorOr<Success>>
{
    const string FAILURE_MESSAGE = "The password for {0} could not be verified.";

    public async Task<ErrorOr<Success>> Handle(VerifyPasswordCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Verifying password for login {Login}.", request.Login);

        var failureMessage = string.Format(FAILURE_MESSAGE, request.Login);

        var hashedPassword = await accountsDatabase.FetchPasswordhash(request.Login);
        if (hashedPassword == null)
        {
            /**
             * I am aware that this `return` is vulnerable to a timing attack.
             * I am not going to mitigate that attack, because it would only 
             * yield a list of usernames, which isn't even a secret for this 
             * project.
             * 
             * HOWEVER: If I use this code in other projects, the scenario may 
             * be different, so I should again consider whether or not to 
             * secure against the possible timing attack here.
             */
            Log.Warning("Password verification for {Login} failed because the account does not exist.", request.Login);
            return Error.Failure(failureMessage);
        }

        var result = new PasswordHasher().VerifyPassword(hashedPassword, request.Password);
        if (result == false)
        {
            Log.Warning("Password verification for {Login} failed. The password was invalid.", request.Login);
            return Error.Failure(failureMessage);
        }

        return Result.Success;
    }
}
