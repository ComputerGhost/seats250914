using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using MediatR;
using Serilog;

namespace Core.Application.Seats;
internal class ListSeatsQueryHandler(ISeatsDatabase seatsDatabase)
    : IRequestHandler<ListSeatsQuery, ListSeatsQueryResponse>
{
    public async Task<ListSeatsQueryResponse> Handle(ListSeatsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<SeatEntityModel> seatEntities;
        if (request.StatusFilter == null)
        {
            Log.Information("Listing all seats.");
            seatEntities = await seatsDatabase.ListSeats();
        }
        else
        {
            var statusFilter = request.StatusFilter.ToString()!;
            Log.Information("Listing seats filtered by {statusFilter}.", statusFilter);
            seatEntities = await seatsDatabase.ListSeats(statusFilter);
        }

        return new ListSeatsQueryResponse
        {
            Data = seatEntities.Select(seatEntity => new ListSeatsQueryResponseItem
            {
                SeatNumber = seatEntity.Number,
                Status = Enum.Parse<SeatStatus>(seatEntity.Status),
            }),
        };
    }
}
