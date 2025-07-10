using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;

namespace Core.Application.Emails;
internal class MarkEmailAsFailedCommandHandler(IEmailsDatabase emailsDatabase)
    : IRequestHandler<MarkEmailAsFailedCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(MarkEmailAsFailedCommand request, CancellationToken cancellationToken)
    {
        return await emailsDatabase.MarkAsFailed(request.EmailId)
            ? Result.Success
            : Error.NotFound();
    }
}
