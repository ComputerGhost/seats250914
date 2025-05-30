using Core.Domain.Common.Models;
using ErrorOr;
using MediatR;

namespace Core.Application.Accounts;
public class UpdateAccountCommand : IRequest<ErrorOr<Success>>
{
    /// <summary>
    /// Login of the user to update.
    /// </summary>
    /// <remarks>
    /// This is used to find the user and is not saved.
    /// </remarks>
    public string Login { get; set; } = null!;

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
