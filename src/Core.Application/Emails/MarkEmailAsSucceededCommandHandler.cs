using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;

namespace Core.Application.Emails;
internal class MarkEmailAsSucceededCommandHandler(IEmailsDatabase emailsDatabase)
    : IRequestHandler<MarkEmailAsSucceededCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(MarkEmailAsSucceededCommand request, CancellationToken cancellationToken)
    {
        return await emailsDatabase.MarkAsSent(request.EmailId)
            ? Result.Success
            : Error.NotFound();
    }
}
