using ErrorOr;
using MediatR;

namespace Core.Application.Emails;
public class MarkEmailAsFailedCommand(int emailId) : IRequest<ErrorOr<Success>>
{
    public int EmailId { get; set; } = emailId;
}
