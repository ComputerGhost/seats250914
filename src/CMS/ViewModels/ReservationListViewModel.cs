using Core.Application.Reservations;
using Core.Domain.Common.Enumerations;
using Presentation.Shared.FrameworkEnhancements.Extensions;

namespace CMS.ViewModels;

public class ReservationListViewModel
{
    public ReservationListViewModel(ListReservationsQueryResponse listReservationsQueryResponse)
    {
        Items = listReservationsQueryResponse.Data.Select(source => new ListItem
        {
            Name = source.Name,
            ReservationId = source.ReservationId,
            ReservedAtDisplay = source.ReservedAt.ToNormalizedString(),
            ReservedAtParameter = source.ReservedAt.ToString("s"),
            ReservationStatus = source.Status,
            SeatNumber = source.SeatNumber,
        });
    }

    public IEnumerable<ListItem> Items { get; set; } = null!;

    /// <summary>
    /// Initial search filter.
    /// </summary>
    public required string Search { get; set; } = null!;

    public struct ListItem
    {
        public string Name { get; set; }
        public int ReservationId { get; set; }
        public string ReservedAtDisplay { get; set; }
        public string ReservedAtParameter { get; set; }
        public ReservationStatus ReservationStatus { get; set; }
        public int SeatNumber { get; set; }
    }
}
