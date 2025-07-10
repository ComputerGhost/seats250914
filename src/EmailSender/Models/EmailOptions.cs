namespace EmailSender.Models;
internal class EmailOptions
{
    public string ServerHost { get; set; } = null!;

    public int ServerPort { get; set; } = 25;

    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Username to sign into sender email account.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password to sign into sender email account.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Sender email address for emails from the system.
    /// </summary>
    public string SenderEmail { get; set; } = null!;

    /// <summary>
    /// Sender name for emails from the system.
    /// </summary>
    public string SenderName { get; set; } = null!;
}
