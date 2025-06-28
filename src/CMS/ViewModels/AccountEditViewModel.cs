using Core.Application.Accounts;
using System.ComponentModel.DataAnnotations;

namespace CMS.ViewModels;

public class AccountEditViewModel
{
    public AccountEditViewModel()
    {
    }

    public AccountEditViewModel(FetchAccountQueryResponse queryResponse)
    {
        Login = queryResponse.Login;
        IsEnabled = queryResponse.IsEnabled;
    }

    public string Action { get; set; } = null!;

    public string Login { get; set; } = null!;

    /* Account edit */

    public bool IsEnabled { get; set; } = true;

    /* Change password */

    public bool IsPasswordChangeSuccessful { get; private set; } = false;

    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public AccountEditViewModel WithSuccessfulPasswordChange()
    {
        IsPasswordChangeSuccessful = true;
        return this;
    }
}
