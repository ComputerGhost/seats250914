using ErrorOr;
using MediatR;

namespace Core.Application.Accounts;
public class VerifyPasswordCommand : IRequest<ErrorOr<Success>>
{
    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;
}
