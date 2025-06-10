using System.ComponentModel.DataAnnotations;

namespace CMS.ViewModels;

public class AccountSignInViewModel
{
    [MaxLength(8)]
    [Required]
    public string Login { get; set; } = null!;

    [DataType(DataType.Password)]
    [MaxLength(50)]
    [Required]
    public string Password { get; set; } = "";
}
