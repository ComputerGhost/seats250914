public class HomeIndexViewModel
{
    public bool Success { get; set; } = true;
    public string? Error { get; set; } = null;

    // Dummy flag indicating if reservations are currently open
    public bool IsOpen { get; set; } = true;

    // Dummy seat reservation data
    public int TotalSeats { get; set; } = 100;
    public int Confirmed { get; set; } = 40;
    public int Pending { get; set; } = 30;
    public int Unassigned { get; set; } = 30;

    // Dummy reservation configuration settings
    public bool IsScheduled { get; set; } = true;  // Whether reservation is scheduled or immediate
    public DateTime? ScheduledOpen { get; set; } = new DateTime(2025, 07, 05, 18, 02, 30);
    public DateTime? ScheduledClose { get; set; } = new DateTime(2025, 07, 06, 23, 59, 00);

    public bool IsOpenNow { get; set; } = false; 
    public int MaxPerUser { get; set; } = 4;
    public int MaxPerIp { get; set; } = 10;
    public int HoldSeconds { get; set; } = 600;
    public int GraceSeconds { get; set; } = 4;
}