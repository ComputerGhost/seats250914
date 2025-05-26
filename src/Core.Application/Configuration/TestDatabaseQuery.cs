using ErrorOr;
using MediatR;

namespace Core.Application.Configuration;
public class TestDatabaseQuery : IRequest<ErrorOr<TestDatabaseQueryResponse>>
{
}
