using ErrorOr;
using MediatR;

namespace Core.Application.Accounts;
public class FetchAccountQuery : IRequest<ErrorOr<FetchAccountQueryResponse>>
{
    public FetchAccountQuery(string login)
    {
        Login = login;
    }

    /// <summary>
    /// Login of the user to fetch.
    /// </summary>
    public string Login { get; set; } = null!;
}
