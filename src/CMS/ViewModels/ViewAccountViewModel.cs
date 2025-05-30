using Core.Application.Accounts;

namespace CMS.ViewModels;

public class ViewAccountViewModel
{
    public ViewAccountViewModel(FetchAccountQueryResponse fetchAccountQueryResponse)
    {
        IsEnabled = fetchAccountQueryResponse.IsEnabled;
        Login = fetchAccountQueryResponse.Login;
    }

    public bool IsEnabled { get; set; }

    public string Login { get; set; } = null!;
}
