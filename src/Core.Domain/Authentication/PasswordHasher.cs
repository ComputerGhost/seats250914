using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace Core.Domain.Authentication;
public class PasswordHasher
{
    private readonly IPasswordHasher<AllUserTypes> _passwordHasher;
    private readonly AllUserTypes _genericUser = new();

    public PasswordHasher()
    {
        _passwordHasher = new PasswordHasher<AllUserTypes>();
    }

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(_genericUser, password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(_genericUser, hashedPassword, providedPassword);
        
        // This shouldn't happen in the short timeframe in which the website will be active.
        // For future projects that reuse this code, please account for this scenario.
        Debug.Assert(result != PasswordVerificationResult.SuccessRehashNeeded);

        return result == PasswordVerificationResult.Success;
    }

    // I don't need the generic parameter of IPasswordHasher, so plug this into it.
    private class AllUserTypes
    {
    };
}
