namespace Core.Domain.Common.Models.Entities;
public class QueuedEmailEntity
{
    public int Id { get; set; }

    public string EmailType { get; set; } = null!;

    public int ReferenceId { get; set; }

    public bool IsSent { get; set; }

    public int AttemptCount { get; set; }

    public DateTimeOffset? LastAttempt { get; set; }
}
