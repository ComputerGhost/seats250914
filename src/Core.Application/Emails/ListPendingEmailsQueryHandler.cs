using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Ports;
using MediatR;
using Serilog;

namespace Core.Application.Emails;
internal class ListPendingEmailsQueryHandler(IEmailsDatabase emailsDatabase)
    : IRequestHandler<ListPendingEmailsQuery, ListPendingEmailsQueryResponse>
{
    public async Task<ListPendingEmailsQueryResponse> Handle(ListPendingEmailsQuery request, CancellationToken cancellationToken)
    {
        Log.Information("Listing pending emails below max retry count of {MaxAttempts}.", request.MaxAttempts);

        var emailEntities = await emailsDatabase.ListPendingEmails(request.MaxAttempts);

        return new ListPendingEmailsQueryResponse
        {
            Data = emailEntities.Select(emailEntity => new ListPendingEmailsQueryResponseItem
            {
                AttemptCount = emailEntity.AttemptCount,
                EmailType = Enum.Parse<EmailType>(emailEntity.EmailType),
                Id = emailEntity.Id,
                LastAttempt = emailEntity.LastAttempt,
                ReferenceId = emailEntity.ReferenceId,
            })
        };
    }
}
