using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
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

    public Task<bool> CreateAccount(AccountEntityModel account, string passwordHash)
    {
        return Task.FromResult(true);
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
