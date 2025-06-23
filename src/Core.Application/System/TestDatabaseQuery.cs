using ErrorOr;
using MediatR;

namespace Core.Application.System;
public class TestDatabaseQuery : IRequest<ErrorOr<TestDatabaseQueryResponse>>
{
}
