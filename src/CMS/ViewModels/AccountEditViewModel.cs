using Core.Application.Accounts;
using System.ComponentModel;
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

    public bool IsPasswordChangeSuccessful { get; set; } = false;

    [MaxLength(50)]
    [Required]
    public string Password { get; set; } = "";

    [Compare(nameof(Password))]
    [DisplayName("Confirm Password")]
    public string ConfirmPassword { get; set; } = "";
}
