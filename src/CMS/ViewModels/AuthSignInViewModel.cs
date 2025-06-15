using System.ComponentModel.DataAnnotations;

namespace CMS.ViewModels;

public class AuthSignInViewModel
{
    public string Login { get; set; } = null!;

    [DataType(DataType.Password)]
    public string Password { get; set; } = "";
}
