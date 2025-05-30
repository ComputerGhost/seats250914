using ErrorOr;
using MediatR;

namespace Core.Application.Accounts;
public class UpdatePasswordCommand : IRequest<ErrorOr<Success>>
{
    public UpdatePasswordCommand(string login, string password)
    {
        Login = login;
        Password = password;
    }

    /// <summary>
    /// Login of the user to update.
    /// </summary>
    public string Login { get; set; } = null!;

    /// <summary>
    /// New user password in plaintext.
    /// </summary>
    public string Password { get; set; } = null!;
}
