using Core.Domain.Common.Models;

namespace Core.Domain.Common.Ports;
public interface IAccountsDatabase
{
    /// <summary>
    /// Creates a new user account.
    /// </summary>
    /// <remarks>
    /// Throws an exception if the login already exists.
    /// </remarks>
    /// <returns>True if the account was created, false otherwise.</returns>
    Task<bool> CreateAccount(AccountEntityModel account, string passwordHash);

    /// <summary>
    /// Fetch an account's data. The password is not included.
    /// </summary>
    /// <returns>The found account, or null if not found.</returns>
    Task<AccountEntityModel?> FetchAccount(string login);

    /// <summary>
    /// Fetch an account's password hash.
    /// </summary>
    /// <returns>The hashed password of the account, or null if not found.</returns>
    Task<string?> FetchPasswordhash(string login);

    /// <summary>
    /// List all accounts. The password is not included.
    /// </summary>
    Task<IEnumerable<AccountEntityModel>> ListAccounts();

    /// <summary>
    /// Updates the account that has the login specified in the model.
    /// </summary>
    /// <remarks>
    /// The `login` property and the password data are not updatd.
    /// </remarks>
    /// <returns>True if the account was updated, false otherwise.</returns>
    Task<bool> UpdateAccount(AccountEntityModel account);

    /// <summary>
    /// Updates the account that has the 
    /// </summary>
    /// <returns>True if the account was updated.</returns>
    Task<bool> UpdatePassword(string login, string passwordHash);
}
