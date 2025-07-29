using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Serilog;
using System.Data;
using System.Diagnostics;
using System.Transactions;

namespace Core.Infrastructure.Database;

[ServiceImplementation(Interface = typeof(IUnitOfWork))]
internal class UnitOfWork(IDbConnection connection) : IUnitOfWork, IDisposable
{
    private bool _closeConnection = false;
    private TransactionScope? _scope = null;

    private IReservationsDatabase? _reservations;
    private ISeatLocksDatabase? _seatLocks;
    private ISeatsDatabase? _seats;

    public IReservationsDatabase Reservations => _reservations ??= new ReservationsDatabase(Connection);
    public ISeatLocksDatabase SeatLocks => _seatLocks ??= new SeatLocksDatabase(Connection);
    public ISeatsDatabase Seats => _seats ??= new SeatsDatabase(Connection);

    private IDbConnection Connection
    {
        get
        {
            Debug.Assert(_scope != null, "The transaction must be started before the unit of work can be used.");
            return connection;
        }
    }

    public void Begin()
    {
        Debug.Assert(_scope == null, "The transaction should not be started twice.");

        Log.Information("Starting transaction for connection with state {State}.", connection.State);

        if (connection.State == ConnectionState.Closed)
        {
            _closeConnection = true;
            connection.Open();
        }

        _scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
    }

    public void Commit()
    {
        Debug.Assert(_scope != null, "The transaction should have been started already here.");

        Log.Information("Commiting transaction.");
        _scope.Complete();
        CleanUp();
    }

    public void Dispose()
    {
        CleanUp();
    }

    public void Rollback()
    {
        Debug.Assert(_scope != null, "The transaction should have been started already here.");

        Log.Information("Rolling back transaction.");
        CleanUp();
    }

    public async Task<T> RunInTransaction<T>(Func<Task<T>> operation)
    {
        Begin();
        try
        {
            var result = await operation();
            Commit();
            return result;
        }
        catch
        {
            Rollback();
            throw;
        }
    }

    private void CleanUp()
    {
        _scope?.Dispose();
        _scope = null;

        if (_closeConnection)
        {
            connection.Close();
            _closeConnection = false;
        }
    }
}
