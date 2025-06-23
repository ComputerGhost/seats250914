using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;

namespace Core.Domain.Scheduling;
public class OpenChecker
{
    public static OpenChecker FromConfiguration(ConfigurationEntityModel configuration)
    {
        return new(configuration, null);
    }

    public static async Task<OpenChecker> FromDatabase(IConfigurationDatabase configuratinDatabase, ISeatsDatabase seatsDatabase)
    {
        var configuration = await configuratinDatabase.FetchConfiguration();
        return new(configuration, seatsDatabase);
    }

    private readonly ISeatsDatabase? _seatsDatabase;

    private OpenChecker(ConfigurationEntityModel configuration, ISeatsDatabase? seatsDatabase)
    {
        IsForcedClosed = configuration.ForceCloseReservations;
        IsForcedOpen = configuration.ForceOpenReservations;
        ScheduledCloseDateTime = configuration.ScheduledCloseDateTime;
        ScheduledCloseTimeZone = configuration.ScheduledCloseTimeZone;
        ScheduledOpenDateTime = configuration.ScheduledOpenDateTime;
        ScheduledOpenTimeZone = configuration.ScheduledOpenTimeZone;
        _seatsDatabase = seatsDatabase;
    }

    public bool IsForcedClosed { get; set; }
    public bool IsForcedOpen { get; set; }

    public DateTimeOffset ScheduledCloseDateTime { get; set; }
    public string ScheduledCloseTimeZone { get; set; }

    public DateTimeOffset ScheduledOpenDateTime { get; set; }
    public string ScheduledOpenTimeZone { get; set; }

    public bool AreReservationsOpen()
    {
        if (IsForcedClosed || IsForcedOpen)
        {
            return IsForcedOpen;
        }

        var now = DateTime.UtcNow;
        return ScheduledOpenDateTime < now && now < ScheduledCloseDateTime;
    }

    public async Task<ReservationsStatus> CalculateStatus()
    {
        if (_seatsDatabase == null)
        {
            throw new InvalidOperationException("The seats database adapter must be specified to use `CalculateStatus`.");
        }

        if (!AreReservationsOpen())
        {
            return IsForcedClosed
                ? ReservationsStatus.ClosedManually
                : ReservationsStatus.ClosedPerSchedule;
        }

        if (await _seatsDatabase.CountSeats(SeatStatus.Available.ToString()) == 0)
        {
            if (await _seatsDatabase.CountSeats(SeatStatus.Locked.ToString()) == 0
                && await _seatsDatabase.CountSeats(SeatStatus.AwaitingPayment.ToString()) == 0)
            {
                return ReservationsStatus.OutOfSeatsPermanently;
            }

            return ReservationsStatus.OutOfSeatsTemporarily;
        }

        return IsForcedOpen
            ? ReservationsStatus.OpenedManually
            : ReservationsStatus.OpenedPerSchedule;
    }
}
