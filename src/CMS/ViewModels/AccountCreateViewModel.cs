using System.ComponentModel.DataAnnotations;

namespace CMS.ViewModels;

public class AccountCreateViewModel
{
    public string Login { get; set; } = "";

    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";

    public bool IsEnabled { get; set; } = true;
}
