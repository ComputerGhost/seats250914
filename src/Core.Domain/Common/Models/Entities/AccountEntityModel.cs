namespace Core.Domain.Common.Models.Entities;
public class AccountEntityModel
{
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Unique identifier of the user.
    /// </summary>
    public string Login { get; set; } = null!;
}
