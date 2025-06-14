namespace Core.Domain.Common.Models;
public class SeatEntityModel
{
    /// <summary>
    /// Unique number to identify the seat.
    /// </summary>
    /// <remarks>
    /// This needs to be consistent across databases.
    /// This probably means that it is not the primary key, 
    /// but that decision is outside the purview of the domain layer.
    /// </remarks>
    public int Number { get; set; }

    /// <summary>
    /// Status of the seat.
    /// </summary>
    public string Status { get; set; } = null!;
}
