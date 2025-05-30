using Core.Domain.Authentication;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;

namespace Core.Application.Accounts;
internal class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, ErrorOr<Success>>
{
    private readonly IAccountsDatabase _accountsDatabase;

    public UpdatePasswordCommandHandler(IAccountsDatabase accountsDatabase)
    {
        _accountsDatabase = accountsDatabase;
    }

    public async Task<ErrorOr<Success>> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        var hashedPassword = new PasswordHasher().HashPassword(request.Password);
        return (await _accountsDatabase.UpdatePassword(request.Login, hashedPassword))
            ? Result.Success : Error.NotFound();
    }
}
