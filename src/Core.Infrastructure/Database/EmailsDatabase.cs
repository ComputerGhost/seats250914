using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Dapper;
using System.Data;

namespace Core.Infrastructure.Database;

[ServiceImplementation]
internal class EmailsDatabase(IDbConnection connection) : IEmailsDatabase
{
    public async Task EnqueueEmail(string emailType, int referenceId)
    {
        var sql = """
            INSERT INTO EmailQueue (EmailTypeId, ReferenceId)
            SELECT EmailTypes.Id, @referenceId
            FROM EmailTypes
            WHERE EmailTypes.Name = @emailType
            """;
        await connection.ExecuteAsync(sql, new
        {
            emailType,
            referenceId,
        });
    }

    public async Task<IEnumerable<QueuedEmailEntity>> ListPendingEmails(int maxAttemptCount)
    {
        var sql = """
            SELECT EmailTypes.Name EmailType, EmailQueue.*
            FROM EmailQueue
            LEFT JOIN EmailTypes ON EmailTypes.Id = EmailQueue.EmailTypeId
            WHERE IsSent = 0 AND EmailQueue.AttemptCount < @maxAttemptCount
            """;
        return await connection.QueryAsync<QueuedEmailEntity>(sql, new
        {
            maxAttemptCount,
        });
    }

    public async Task<bool> MarkAsFailed(int emailId)
    {
        var now = DateTimeOffset.UtcNow;
        var sql = """
            UPDATE EmailQueue SET
                AttemptCount = AttemptCount + 1,
                LastAttempt = @now
            WHERE Id = @emailId
            """;
        return await connection.ExecuteAsync(sql, new
        {
            emailId,
            now,
        }) > 0;
    }

    public async Task<bool> MarkAsSent(int emailId)
    {
        var now = DateTimeOffset.UtcNow;
        var sql = """
            UPDATE EmailQueue SET
                AttemptCount = AttemptCount + 1,
                LastAttempt = @now,
                IsSent = 1
            WHERE Id = @emailId
            """;
        return await connection.ExecuteAsync(sql, new
        {
            emailId,
            now,
        }) > 0;
    }
}
