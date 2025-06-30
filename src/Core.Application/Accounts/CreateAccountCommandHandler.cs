using Core.Domain.Authentication;
using Core.Domain.Common.Exceptions;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Accounts;
internal class CreateAccountCommandHandler(IAccountsDatabase accountsDatabase)
    : IRequestHandler<CreateAccountCommand, ErrorOr<Created>>
{
    public async Task<ErrorOr<Created>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Creating account {Login}.", request.Login);
        Log.Debug("Account data being saved is {@request}", request);

        try
        {
            var hashedPassword = HashPassword(request.Password);
            await CreateAccount(request, hashedPassword);
            return Result.Created;
        }
        catch (AccountAlreadyExistsException)
        {
            Log.Warning("The account {Login} could not be created because it already exists.", request.Login);
            return Error.Conflict($"An account already exists with the login '{request.Login}'.");
        }
    }

    private async Task CreateAccount(CreateAccountCommand request, string hashedPassword)
    {
        var accountEntity = request.ToAccountEntityModel();
        await accountsDatabase.CreateAccount(accountEntity, hashedPassword);
    }

    private static string HashPassword(string password)
    {
        var hasher = new PasswordHasher();
        return hasher.HashPassword(password);
    }
}
