using Core.Application.Accounts;
using Core.Domain.Authentication;
using Core.Infrastructure;
using Dapper;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CMS.IntegrationTests;
internal static class TestDataSetup
{
    /// <summary>
    /// A user account that is safe to mess with.
    /// </summary>
    public const string TEST_USER_LOGIN = "autotest";

    private static string ConnectionString => ConfigurationAccessor.Instance.Services.GetService<IOptions<InfrastructureOptions>>()!.Value.DatabaseConnectionString;
    private static IMediator Mediator => ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;

    public static async Task<string> CreateTestAccount(bool enabled = true)
    {
        var creationResult = await Mediator.Send(new CreateAccountCommand
        {
            Login = TEST_USER_LOGIN,
            Password = GenerateSecurePassword(),
            IsEnabled = enabled,
        });

        // If the user already exists, reset it to the defaults.
        if (creationResult.IsError)
        {
            await Mediator.Send(new UpdateAccountCommand
            {
                Login = TEST_USER_LOGIN,
                IsEnabled = enabled,
            });
        }

        return TEST_USER_LOGIN;
    }

    public static async Task DeleteTestAccount()
    {
        var sql = "DELETE FROM Users WHERE Login = @login";
        using var connection = new SqlConnection(ConnectionString);
        await connection.ExecuteAsync(sql, new
        {
            login = TEST_USER_LOGIN,
        });
    }

    public static string GenerateSecurePassword()
    {
        // Reuse seat key generations since that's secure random.
        // We don't want to use constant or simple strings for this,
        // just in case it is foolishly run on production.
        return SeatKeyUtilities.GenerateKey();
    }
}
