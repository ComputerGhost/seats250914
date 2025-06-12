using Core.Domain.Authentication;
using Core.Domain.Common.Exceptions;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;

namespace Core.Application.Accounts;
internal class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, ErrorOr<Created>>
{
    private readonly IAccountsDatabase _accountsDatabase;

    public CreateAccountCommandHandler(IAccountsDatabase accountsDatabase)
    {
        _accountsDatabase = accountsDatabase;
    }

    public async Task<ErrorOr<Created>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var hashedPassword = HashPassword(request.Password);
            await CreateAccount(request, hashedPassword);
            return Result.Created;
        }
        catch (AccountAlreadyExistsException)
        {
            return Error.Conflict($"An account already exists with the login '{request.Login}'.");
        }
    }

    private async Task CreateAccount(CreateAccountCommand request, string hashedPassword)
    {
        var accountEntity = request.ToAccountEntityModel();
        await _accountsDatabase.CreateAccount(accountEntity, hashedPassword);
    }

    private static string HashPassword(string password)
    {
        var hasher = new PasswordHasher();
        return hasher.HashPassword(password);
    }
}
