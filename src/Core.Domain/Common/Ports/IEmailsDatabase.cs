using Core.Domain.Common.Models.Entities;

namespace Core.Domain.Common.Ports;
public interface IEmailsDatabase
{
    /// <summary>
    /// Enqueues an email to be sent.
    /// </summary>
    /// <param name="emailType">A valid email type.</param>
    /// <param name="referenceId">Primary key of referenced item. Resolved item type varies per email type.</param>
    public Task EnqueueEmail(string emailType, int referenceId);

    /// <summary>
    /// Lists all seats that have not yet been sent.
    /// </summary>
    /// <param name="maxAttemptCount">Excludes emails that failed this many times.</param>
    public Task<IEnumerable<QueuedEmailEntity>> ListPendingEmails(int maxAttemptCount);

    /// <summary>
    /// Mark an email as failing to be sent.
    /// </summary>
    /// <returns>
    /// True if email was found and updated.
    /// </returns>
    public Task<bool> MarkAsFailed(int emailId);

    /// <summary>
    /// Mark an email as being successfully sent.
    /// </summary>
    /// <returns>
    /// True if email was found and updated.
    /// </returns>
    public Task<bool> MarkAsSent(int emailId);
}
