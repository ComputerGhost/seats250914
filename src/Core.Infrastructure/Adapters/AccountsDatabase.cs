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

    public Task<AccountEntityModel?> FetchAccount(string login)
    {
        var user = new AccountEntityModel
        {
            IsEnabled = false,
            Login = "<script>alert('해킹 테스트')</script>",
        };
        return Task.FromResult<AccountEntityModel?>(user);
    }

    public Task<IEnumerable<AccountEntityModel>> ListAccounts()
    {
        IEnumerable<AccountEntityModel> mock = [
            new AccountEntityModel {
                IsEnabled = true,
                Login = "한글 테스트",
            },
            new AccountEntityModel {
                IsEnabled = true,
                Login = "test2",
            },
            new AccountEntityModel {
                IsEnabled = false,
                Login = "<script>alert('JS injection test')</script>",
            },
        ];
        return Task.FromResult(mock);
    }

    public Task<bool> UpdateAccount(AccountEntityModel account)
    {
        return Task.FromResult(true);
    }

    public Task<bool> UpdatePassword(string login, string passwordHash)
    {
        return Task.FromResult(true);
    }
}
