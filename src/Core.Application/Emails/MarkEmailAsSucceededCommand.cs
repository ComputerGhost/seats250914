using ErrorOr;
using MediatR;

namespace Core.Application.Emails;
public class MarkEmailAsSucceededCommand(int emailId) : IRequest<ErrorOr<Success>>
{
    public int EmailId { get; set; } = emailId;
}
