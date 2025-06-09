using Core.Domain.Common.Exceptions;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Core.Infrastructure.Adapters;

[ServiceImplementation]
internal class AccountsDatabase : IAccountsDatabase
{
    private readonly IDbConnection _connection;

    public AccountsDatabase(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<bool> CreateAccount(AccountEntityModel account, string passwordHash)
    {
        try
        {
            var sql = """
                INSERT INTO [Users] (Login, PasswordHash, IsEnabled)
                VALUES (@Login, @PasswordHash, @IsEnabled);
                """;
            return await _connection.ExecuteAsync(sql, new
            {
                account.Login,
                account.IsEnabled,
                PasswordHash = passwordHash,
            }) > 0;
        }
        catch (SqlException ex) when (ex.Number == 2601) // Duplicate error
        {
            throw new AccountAlreadyExistsException(account.Login);
        }
    }

    public async Task<AccountEntityModel?> FetchAccount(string login)
    {
        var sql = "SELECT Login, IsEnabled FROM [Users] WHERE Login = @login";
        return await _connection.QuerySingleOrDefaultAsync<AccountEntityModel>(sql, new
        {
            Login = login,
        });
    }

    public async Task<IEnumerable<AccountEntityModel>> ListAccounts()
    {
        var sql = "SELECT Login, IsEnabled FROM [Users] ORDER BY Login";
        return await _connection.QueryAsync<AccountEntityModel>(sql);
    }

    public async Task<bool> UpdateAccount(AccountEntityModel account)
    {
        var sql = "UPDATE [Users] SET IsEnabled = @isEnabled WHERE Login = @login";
        return await _connection.ExecuteAsync(sql, new
        {
            isEnabled = account.IsEnabled,
            login = account.Login,
        }) > 0;
    }

    public async Task<bool> UpdatePassword(string login, string passwordHash)
    {
        var sql = "UPDATE [Users] SET PasswordHash = @passwordHash WHERE Login = @login";
        return await _connection.ExecuteAsync(sql, new
        {
            login,
            passwordHash,
        }) > 0;
    }
}
