using Core.Application.Reservations;
using Core.Application.Seats;
using Core.Application.System;
using Core.Domain.Authorization;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.FrameworkEnhancements.Extensions;
using Presentation.Shared.LockCleanup;
using Public.Extensions;
using Public.Models.DTOs;
using System.Diagnostics;

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
            SeatNumber = request.SeatNumber,
        });

        if (lockResult.IsError)
        {
            var error = lockResult.FirstError;
            return error.Type switch
            {
                ErrorType.Conflict => Results.Conflict(),
                ErrorType.NotFound => Results.NotFound(),
                ErrorType.Unauthorized => Unauthorized(error),
                _ => throw new NotImplementedException(),
            };
        }

        var cleanupScheduler = HttpContext.RequestServices
            .GetServices<IHostedService>()
            .OfType<CleanupScheduler>()
            .Single();
        await cleanupScheduler.ScheduleCleanup();

        return Results.Ok(lockResult.Value);
    }

    private static IResult Unauthorized(Error authError)
    {
        Debug.Assert(authError.Metadata != null);
        Debug.Assert(authError.Metadata["details"] != null);
        var authResult = (AuthorizationResult)authError.Metadata["details"];

        return Results.Json(
            authResult,
            statusCode: StatusCodes.Status403Forbidden
        );
    }
}
