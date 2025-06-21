using Core.Application.Reservations;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.FrameworkEnhancements.Extensions;
using Presentation.Shared.LockCleanup;
using Public.Models.DTOs;

namespace Public.Controllers;

[Route("api")]
public class ApiController(IMediator mediator) : Controller
{
    [HttpPost("lock-seat")]
    public async Task<IResult> LockSeat([FromBody] LockSeatRequest request)
    {
        var ipAddress = Request.GetClientIpAddress();
        var lockResult = await LockSeat(ipAddress, request.SeatNumber);

        return lockResult.Match(
            result => Results.Ok(result),
            error => error.First().Type switch
            {
                ErrorType.NotFound => Results.NotFound(),
                ErrorType.Conflict => Results.Conflict(),
                _ => throw new NotImplementedException(),
            });
    }

    private async Task<ErrorOr<LockSeatCommandResponse>> LockSeat(string ipAddress, int seatNumber)
    {
        var lockResult = await mediator.Send(new LockSeatCommand
        {
            IpAddress = ipAddress,
            IsStaff = false,
            SeatNumber = seatNumber,
        });

        var cleanupScheduler = HttpContext.RequestServices
            .GetServices<IHostedService>()
            .OfType<CleanupScheduler>()
            .Single();
        await cleanupScheduler.ScheduleCleanup();

        return lockResult;
    }
}
