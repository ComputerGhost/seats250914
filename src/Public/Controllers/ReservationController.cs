using Core.Application.Reservations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Presentation.Shared.FrameworkEnhancements.Extensions;
using Public.Models;
using Public.Models.ViewModels;
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
    public IActionResult ReserveSeats([FromServices] IOptions<Config> config)
    {
        var seatLock = GetSeatLocksFromCookie();
        if (seatLock == null || seatLock.LockExpiration < DateTime.UtcNow)
        {
            // The seat lock won't be expired here in the normal flow,
            // so let's redirect to "/" instead of the expiration page.
            return RedirectToAction("Index", "Home");
        }

        return View(new ReserveSeatsViewModel
        {
            OrganizerEmail = config.Value.OrganizerEmail,
            SeatNumbers = seatLock.SeatLocks.Keys,
        }.WithExpiration(seatLock.LockExpiration));
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReserveSeats([FromForm] ReserveSeatsViewModel model)
    {
        return model.Action switch
        {
            "submit" => await ReserveSeats_Submit(model),
            "cancel" => await ReserveSeats_Cancel(),
            _ => throw new NotImplementedException(),
        };
    }

    private async Task<IActionResult> ReserveSeats_Cancel()
    {
        var seatLocks = GetSeatLocksFromCookie();
        if (seatLocks != null)
        {
            await mediator.Send(new UnlockSeatsCommand
            {
                SeatLocks = seatLocks.SeatLocks,
            });

            Response.Cookies.Delete("seatLocks");
        }

        return RedirectToAction("Index", "Home");
    }

    private async Task<IActionResult> ReserveSeats_Submit(ReserveSeatsViewModel model)
    {
        // Frontend should've handled this, but double-check here.
        if (!model.AgreeToTerms)
        {
            return BadRequest();
        }

        var seatLocks = GetSeatLocksFromCookie();
        if (seatLocks == null)
        {
            return RedirectToAction(nameof(TimeExpired));
        }

        // The seat lock will be verified by the reservation code.
        var ipAddress = Request.GetClientIpAddress();
        var result = await mediator.Send(model.ToReserveSeatsCommand(ipAddress, seatLocks));
        if (result.IsError)
        {
            model.SeatNumbers = seatLocks.SeatLocks.Keys;
            model.WithExpiration(seatLocks.LockExpiration);
            model.WithError(result.FirstError);
            return View(model);
        }

        Response.Cookies.Delete("seatLock");

        return RedirectToAction(nameof(MakePayment));
    }

    [HttpGet("payment")]
    public IActionResult MakePayment([FromServices] IOptions<Config> options)
    {
        return View(new MakePaymentViewModel
        {
            PaymentFormUrl = options.Value.PaymentFormUrl,
        });
    }

    /// <summary>
    /// Get the seat locks that the user has saved.
    /// </summary>
    /// <remarks>
    /// User-provided data should not be trusted without being verified,
    /// but it's safe to use it for display purposes.
    /// </remarks>
    private LockSeatsCommandResponse? GetSeatLocksFromCookie()
    {
        if (!Request.Cookies.TryGetValue("seatLocks", out var cookie))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<LockSeatsCommandResponse>(cookie, readOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
