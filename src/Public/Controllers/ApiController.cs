using Core.Application.Reservations;
using Core.Application.Seats;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.FrameworkEnhancements.Extensions;
using Presentation.Shared.LockCleanup;
using Public.Features.SeatSelection.Extensions;
using Public.Models.DTOs;

namespace Public.Controllers;

[Route("api")]
public class ApiController(IMediator mediator) : Controller
{
    [HttpGet("seat-statuses")]
    public async Task<IResult> FetchSeats()
    {
        var allSeats = await mediator.Send(new ListSeatsQuery());
        var statuses = allSeats.Data.ToDictionary(
            v => v.SeatNumber,
            v => v.Status.ToCssClass());
        return Results.Ok(statuses);
    }

    [HttpPost("lock-seat")]
    public async Task<IResult> LockSeat([FromBody] LockSeatRequest request)
    {
        var ipAddress = Request.GetClientIpAddress();
        var lockResult = await mediator.Send(new LockSeatCommand
        {
            IpAddress = ipAddress,
            IsStaff = false,
            SeatNumber = request.SeatNumber,
        });

        var cleanupScheduler = HttpContext.RequestServices
            .GetServices<IHostedService>()
            .OfType<CleanupScheduler>()
            .Single();
        await cleanupScheduler.ScheduleCleanup();

        return lockResult.Match(
            result => Results.Ok(result),
            error => error.First().Type switch
            {
                ErrorType.NotFound => Results.NotFound(),
                ErrorType.Conflict => Results.Conflict(),
                _ => throw new NotImplementedException(),
            });
    }
}
