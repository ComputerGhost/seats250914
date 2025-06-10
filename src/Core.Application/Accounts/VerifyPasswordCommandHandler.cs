using Core.Domain.Authentication;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;

namespace Core.Application.Accounts;
internal class VerifyPasswordCommandHandler : IRequestHandler<VerifyPasswordCommand, ErrorOr<Success>>
{
    const string FAILURE_MESSAGE = "The password for {0} could not be verified.";

    private readonly IAccountsDatabase _accountsDatabase;

    public VerifyPasswordCommandHandler(IAccountsDatabase accountsDatabase)
    {
        _accountsDatabase = accountsDatabase;
    }

    public async Task<ErrorOr<Success>> Handle(VerifyPasswordCommand request, CancellationToken cancellationToken)
    {
        var failureMessage = string.Format(FAILURE_MESSAGE, request.Login);

        var hashedPassword = await _accountsDatabase.FetchPasswordhash(request.Login);
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
            return Error.Failure(failureMessage);
        }

        var result = new PasswordHasher().VerifyPassword(hashedPassword, request.Password);
        if (result == false)
        {
            return Error.Failure(failureMessage);
        }

        return Result.Success;
    }
}
