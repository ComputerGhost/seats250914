using Core.Domain.Common.Models.Entities;
using ErrorOr;
using MediatR;

namespace Core.Application.Accounts;
public class CreateAccountCommand : IRequest<ErrorOr<Created>>
{
    /// <summary>
    /// Login of the user to create. Must be unique.
    /// </summary>
    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool IsEnabled { get; set; }

    internal AccountEntityModel ToAccountEntityModel()
    {
        return new AccountEntityModel
        {
            IsEnabled = IsEnabled,
            Login = Login,
        };
    }
}
