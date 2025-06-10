using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace CMS.Features.Authentication;

/// <summary>
/// Use this with a verified user.
/// </summary>
public class AuthenticationService
{
    private readonly HttpContext _httpContext;

    public AuthenticationService(HttpContext httpContext)
    {
        _httpContext = httpContext;
    }

    public string? Username => _httpContext.User.Identity?.Name;

    public Task SignIn(string name)
    {
        IEnumerable<Claim> claims = [
            new Claim(ClaimTypes.Name, name),
        ];
        var identity = new ClaimsIdentity(claims, "MyAuthenticationScheme");
        var principal = new ClaimsPrincipal(identity);
        return _httpContext.SignInAsync(principal);
    }

    public Task SignOut()
    {
        return _httpContext.SignOutAsync();
    }
}
