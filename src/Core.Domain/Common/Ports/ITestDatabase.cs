namespace Core.Domain.Common.Ports;
public interface ITestDatabase
{
    Task PingDatabase();
}
