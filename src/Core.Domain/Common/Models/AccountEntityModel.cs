namespace Core.Domain.Common.Models;
public class AccountEntityModel
{
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Unique identifier of the user.
    /// </summary>
    public string Login { get; set; } = null!;
}
