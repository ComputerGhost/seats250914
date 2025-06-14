using Core.Domain.Common.Exceptions;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Core.Infrastructure.Adapters;

[ServiceImplementation]
internal class AccountsDatabase(IDbConnection connection) : IAccountsDatabase
{
    public async Task CreateAccount(AccountEntityModel account, string passwordHash)
    {
        try
        {
            var sql = """
                INSERT INTO [Users] (Login, PasswordHash, IsEnabled)
                VALUES (@Login, @PasswordHash, @IsEnabled);
                """;
            await connection.ExecuteAsync(sql, new
            {
                account.Login,
                account.IsEnabled,
                PasswordHash = passwordHash,
            });
        }
        // Catch unique constraint violations.
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            throw new AccountAlreadyExistsException(account.Login);
        }
    }

    public async Task<AccountEntityModel?> FetchAccount(string login)
    {
        var sql = "SELECT Login, IsEnabled FROM [Users] WHERE Login = @login";
        return await connection.QuerySingleOrDefaultAsync<AccountEntityModel>(sql, new
        {
            Login = login,
        });
    }

    public async Task<string?> FetchPasswordhash(string login)
    {
        var sql = "SELECT PasswordHash from [Users] WHERE Login = @login AND IsEnabled = 1";
        return await connection.QuerySingleOrDefaultAsync<string>(sql, new
        {
            Login = login,
        });
    }

    public async Task<IEnumerable<AccountEntityModel>> ListAccounts()
    {
        var sql = "SELECT Login, IsEnabled FROM [Users] ORDER BY Login";
        return await connection.QueryAsync<AccountEntityModel>(sql);
    }

    public async Task<bool> UpdateAccount(AccountEntityModel account)
    {
        var sql = "UPDATE [Users] SET IsEnabled = @isEnabled WHERE Login = @login";
        return await connection.ExecuteAsync(sql, new
        {
            isEnabled = account.IsEnabled,
            login = account.Login,
        }) > 0;
    }

    public async Task<bool> UpdatePassword(string login, string passwordHash)
    {
        var sql = "UPDATE [Users] SET PasswordHash = @passwordHash WHERE Login = @login";
        return await connection.ExecuteAsync(sql, new
        {
            login,
            passwordHash,
        }) > 0;
    }
}
