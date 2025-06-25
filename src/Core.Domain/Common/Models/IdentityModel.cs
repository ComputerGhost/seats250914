namespace Core.Domain.Common.Models;
public class IdentityModel
{
    public string? Email { get; set; }
    public required string IpAddress { get; set; }
    public required bool IsStaff { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PreferredLanguage { get; set; }
}
