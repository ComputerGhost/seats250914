namespace CMS.Features.Authentication;

public class AuthenticationOptions
{
    public string InitialUsername { get; set; } = null!;
    public string InitialPassword { get; set; } = null!;
    public string LoginPath { get; set; } = null!;
}
