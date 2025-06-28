using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Ports;
using MediatR;

namespace Core.Application.Seats;
internal class ListSeatsQueryHandler : IRequestHandler<ListSeatsQuery, ListSeatsQueryResponse>
{
    private readonly ISeatsDatabase _seatsDatabase;

    public ListSeatsQueryHandler(ISeatsDatabase seatsDatabase)
    {
        _seatsDatabase = seatsDatabase;
    }

    public async Task<ListSeatsQueryResponse> Handle(ListSeatsQuery request, CancellationToken cancellationToken)
    {
        var seatEntities = (request.StatusFilter == null)
            ? await _seatsDatabase.ListSeats()
            : await _seatsDatabase.ListSeats(request.StatusFilter.ToString()!);

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
