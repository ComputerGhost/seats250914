using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Presentation.Shared.Authentication;

/// <summary>
/// Use this with a verified user.
/// </summary>
public class AuthenticationService(HttpContext httpContext)
{
    public string? Username => httpContext.User.Identity?.Name;

    public Task SignIn(string name)
    {
        IEnumerable<Claim> claims = [
            new Claim(ClaimTypes.Name, name),
        ];
        var identity = new ClaimsIdentity(claims, "MyAuthenticationScheme");
        var principal = new ClaimsPrincipal(identity);
        return httpContext.SignInAsync(principal);
    }

    public Task SignOut()
    {
        return httpContext.SignOutAsync();
    }
}
