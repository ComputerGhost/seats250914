namespace Core.Domain.Common.Ports;
public interface IUnitOfWork
{
    public IReservationsDatabase Reservations { get; }
    public ISeatLocksDatabase SeatLocks { get; }
    public ISeatsDatabase Seats { get; }

    void Begin();
    void Commit();
    void Rollback();
    Task<T> RunInTransaction<T>(Func<Task<T>> operation);
}
