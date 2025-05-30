using Core.Domain.Authentication;
using Core.Domain.Common.Exceptions;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using System.Diagnostics;

namespace Core.Application.Accounts;
internal class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, ErrorOr<Success>>
{
    private readonly IAccountsDatabase _accountsDatabase;

    public CreateAccountCommandHandler(IAccountsDatabase accountsDatabase)
    {
        _accountsDatabase = accountsDatabase;
    }

    public async Task<ErrorOr<Success>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var hashedPassword = HashPassword(request.Password);
            await CreateAccount(request, hashedPassword);
            return Result.Success;
        }
        catch (AccountAlreadyExistsException)
        {
            return Error.Conflict($"An account already exists with the login '{request.Login}'.");
        }
    }

    private async Task CreateAccount(CreateAccountCommand request, string hashedPassword)
    {
        var accountEntity = request.ToAccountEntityModel();
        var isSuccess = await _accountsDatabase.CreateAccount(accountEntity, hashedPassword);
        Debug.Assert(isSuccess, "Account creation should not have a non-exceptional failure.");
    }

    private static string HashPassword(string password)
    {
        var hasher = new PasswordHasher();
        return hasher.HashPassword(password);
    }
}
