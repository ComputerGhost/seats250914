using Core.Application.Accounts;

namespace CMS.ViewModels;

public class AccountViewViewModel
{
    public AccountViewViewModel(FetchAccountQueryResponse fetchAccountQueryResponse)
    {
        IsEnabled = fetchAccountQueryResponse.IsEnabled;
        Login = fetchAccountQueryResponse.Login;
    }

    public bool IsEnabled { get; set; }

    public string Login { get; set; } = null!;
}
