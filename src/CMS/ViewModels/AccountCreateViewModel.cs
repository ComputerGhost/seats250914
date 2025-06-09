using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CMS.ViewModels;

public class AccountCreateViewModel
{
    [MaxLength(8)]
    [Required]
    public string Login { get; set; } = "";

    [MaxLength(50)]
    [Required]
    public string Password { get; set; } = "";

    [Compare(nameof(Password))]
    [DisplayName("Confirm Password")]
    public string ConfirmPassword { get; set; } = "";

    public bool IsEnabled { get; set; } = true;
}
