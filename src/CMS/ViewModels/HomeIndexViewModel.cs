using Core.Application.Seats;
using Core.Application.System;
using Core.Domain.Common.Enumerations;
using Presentation.Shared.FrameworkEnhancements.Extensions;
using System.Diagnostics;

namespace CMS.ViewModels;
public class HomeIndexViewModel
{
    public HomeIndexViewModel(FetchConfigurationQueryResponse configuration, ListSeatsQueryResponse seats)
    {
        Success = true; // Always true. If the db fails, we won't get here.

        IsOpen = configuration.AreReservationsOpen;

        TotalSeats = seats.Data.Count();
        Confirmed = seats.Data.Where(s => s.Status == SeatStatus.ReservationConfirmed).Count();
        Pending = seats.Data.Where(s => s.Status == SeatStatus.AwaitingPayment).Count();
        Unassigned = seats.Data.Where(s => s.Status == SeatStatus.Available || s.Status == SeatStatus.Locked).Count();
        Debug.Assert(Unassigned + Pending + Confirmed == TotalSeats, "A seat status is not being counted.");

        IsScheduled = !configuration.ForceOpenReservations && !configuration.ForceCloseReservations;
        ScheduledClose = configuration.ScheduledCloseDateTime;
        ScheduledCloseTimeZone = configuration.ScheduledCloseTimeZone;
        ScheduledOpen = configuration.ScheduledOpenDateTime;
        ScheduledOpenTimeZone = configuration.ScheduledOpenTimeZone;

        IsOpenNow = configuration.AreReservationsOpen;
        MaxPerIp = configuration.MaxSeatsPerIPAddress;
        MaxPerIp = configuration.MaxSeatsPerIPAddress;
        HoldSeconds = configuration.MaxSecondsToConfirmSeat;
        GraceSeconds = configuration.GracePeriodSeconds;
    }

    public bool Success { get; set; }

    // If reservations are currently open
    public bool IsOpen { get; set; } = true;

    // Seat reservation data
    public int TotalSeats { get; set; }
    public int Confirmed { get; set; }
    public int Pending { get; set; }
    public int Unassigned { get; set; }

    // Reservation configuration settings
    public bool IsScheduled { get; set; }  // Whether reservation is scheduled or immediate
    public DateTimeOffset ScheduledOpen { get; set; }
    public string ScheduledOpenTimeZone { get; set; }
    public string ScheduledOpenDisplay => ScheduledOpen.ToNormalizedString(ScheduledOpenTimeZone);
    public DateTimeOffset ScheduledClose { get; set; }
    public string ScheduledCloseTimeZone { get; set; }
    public string ScheduledCloseDisplay => ScheduledClose.ToNormalizedString(ScheduledCloseTimeZone);

    public bool IsOpenNow { get; set; }
    public int MaxPerUser { get; set; }
    public int MaxPerIp { get; set; }
    public int HoldSeconds { get; set; }
    public int GraceSeconds { get; set; }
}