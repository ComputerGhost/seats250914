using Core.Application.Reservations;
using Core.Domain.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Presentation.Shared.FrameworkEnhancements.Extensions;
using Public.Models;
using Public.Models.ViewModels;
using System.Diagnostics;
using System.Text.Json;

namespace Public.Controllers;

[Route("reservation")]
public class ReservationController(IMediator mediator) : Controller
{
    private static readonly JsonSerializerOptions readOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [HttpGet("expired")]
    public IActionResult TimeExpired()
    {
        Response.Cookies.Delete("seatLock");
        return View();
    }

    [HttpGet("new")]
    public IActionResult ReserveSeat()
    {
        var seatLock = GetSeatLockFromCookie();
        if (seatLock == null || seatLock.LockExpiration < DateTime.UtcNow)
        {
            // The seat lock won't be expired here in the normal flow,
            // so let's redirect to "/" instead of the expiration page.
            return RedirectToAction("Index", "Home");
        }

        // Trust the lock from the cookie since it's only used for display.
        var timeUntilExpiration = seatLock.LockExpiration - DateTimeOffset.UtcNow;

        return View(new ReserveSeatViewModel
        {
            SeatNumber = seatLock.SeatNumber,
            TimeUntilExpiration = timeUntilExpiration,
        });
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReserveSeat([FromForm] ReserveSeatViewModel model)
    {
        return model.Action switch
        {
            "submit" => await ReserveSeat_Submit(model),
            "cancel" => await ReserveSeat_Cancel(),
            _ => throw new NotImplementedException(),
        };
    }

    private async Task<IActionResult> ReserveSeat_Cancel()
    {
        var seatLock = GetSeatLockFromCookie();
        if (seatLock != null)
        {
            await mediator.Send(new UnlockSeatCommand
            {
                SeatKey = seatLock.SeatKey,
                SeatNumber = seatLock.SeatNumber,
            });

            Response.Cookies.Delete("seatLock");
        }

        return RedirectToAction("Index", "Home");
    }

    private async Task<IActionResult> ReserveSeat_Submit(ReserveSeatViewModel model)
    {
        var seatLock = GetSeatLockFromCookie();
        if (seatLock == null)
        {
            return RedirectToAction(nameof(TimeExpired));
        }

        // The seat lock will be verified by the reservation code.
        var ipAddress = Request.GetClientIpAddress();
        var command = model.ToReserveSeatCommand(ipAddress, seatLock);
        var result = await mediator.Send(command);
        if (result.IsError)
        {
            Debug.Assert(result.FirstError.Metadata != null);
            Debug.Assert(result.FirstError.Metadata["details"] != null);
            var authResult = (AuthorizationResult)result.FirstError.Metadata["details"];
            model.FailureReason = authResult.FailureReason;
            return View(model);
        }

        Response.Cookies.Delete("seatLock");

        return RedirectToAction(nameof(MakePayment));
    }

    [HttpGet("payment")]
    public IActionResult MakePayment([FromServices] IOptions<PaymentConfig> options)
    {
        return View(new MakePaymentViewModel
        {
            PaymentFormUrl = options.Value.PaymentFormUrl,
        });
    }

    /// <summary>
    /// Get the seat lock that the user has saved.
    /// </summary>
    /// <remarks>
    /// User-provided data should not be trusted without being verified,
    /// but it's safe to use it for display purposes.
    /// </remarks>
    private LockSeatCommandResponse? GetSeatLockFromCookie()
    {
        if (!Request.Cookies.TryGetValue("seatLock", out var cookie))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<LockSeatCommandResponse>(cookie, readOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
