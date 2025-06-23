using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;

namespace Core.Application.System;
internal class TestDatabaseQueryHandler : IRequestHandler<TestDatabaseQuery, ErrorOr<TestDatabaseQueryResponse>>
{
    private readonly ITestDatabase _testDatabase;

    public TestDatabaseQueryHandler(ITestDatabase testDatabase)
    {
        _testDatabase = testDatabase;
    }

    public async Task<ErrorOr<TestDatabaseQueryResponse>> Handle(TestDatabaseQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await _testDatabase.PingDatabase();
            return new TestDatabaseQueryResponse();
        }
        catch (Exception ex)
        {
            return Error.Failure(description: ex.Message);
        }
    }
}
