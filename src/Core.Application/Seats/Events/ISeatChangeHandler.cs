namespace Core.Application.Seats.Events;
public interface ISeatChangeHandler
{
    /// <summary>
    /// At least one seat status was updated.
    /// </summary>
    /// <returns></returns>
    Task OnSeatStatusesChanged();
}
