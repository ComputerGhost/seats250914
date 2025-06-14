using System.Diagnostics;
using System.Security.Cryptography;

namespace Core.Domain.Authentication;

/// <summary>
/// Helps to authenticate people who have placed a hold on a seat.
/// </summary>
public class SeatKeyUtilities
{
    public static string GenerateKey(int bitsOfEntropy = 256)
    {
        Debug.Assert(bitsOfEntropy % 8 == 0, "Only bits in multiples of 8 are allowed.");
        var bytes = RandomNumberGenerator.GetBytes(bitsOfEntropy / 8);
        return Convert.ToBase64String(bytes);
    }

    public static bool VerifyKey(string actualKey, string providedKey)
    {
        // I know this is uber simple, but we want calling code to be oblivious to the implementation of the key.
        return actualKey == providedKey;
    }
}
