using Core.Domain.Common.Enumerations;

namespace Core.Application.Emails;
public class ListPendingEmailsQueryResponseItem
{
    public required int Id { get; set; }

    public required EmailType EmailType { get; set; }

    public required int ReferenceId { get; set; }

    public required int AttemptCount { get; set; }

    public required DateTimeOffset? LastAttempt { get; set; }
}
