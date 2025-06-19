using Core.Application.Reservations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Public.Extensions;
using Public.Models.ViewModels;
using System.Text.Json;

namespace Public.Controllers;

[Route("reservation")]
public class ReservationController(IMediator mediator) : Controller
{
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
            return RedirectToAction("Index", "Home");
        }

        // There is no need to verify the seat lock here.
        // If the user has tampered with it, no harm done.
        // We're just showing a page.

        return View(new ReserveSeatViewModel
        {
            SeatNumber = seatLock.SeatNumber,
        });
    }

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReserveSeat(ReserveSeatViewModel model)
    {
        var seatLock = GetSeatLockFromCookie();
        if (seatLock == null || seatLock.LockExpiration < DateTime.UtcNow)
        {
            return RedirectToAction("Index", "Home");
        }

        // There is no need to verify the seat lock here.
        // If the user has tampered with it, the reservation will just fail.
        // It's the same result as it expiring.

        var ipAddress = Request.GetClientIpAddress();
        var command = model.ToReserveSeatCommand(ipAddress, seatLock);
        var result = await mediator.Send(command);
        return result.Match(
            result => Reserved(),
            error => RedirectToAction(nameof(TimeExpired)));

        IActionResult Reserved()
        {
            Response.Cookies.Delete("seatLock");
            return RedirectToAction(nameof(MakePayment));
        }
    }

    [HttpGet("payment")]
    public IActionResult MakePayment()
    {
        return View();
    }

    private LockSeatCommandResponse? GetSeatLockFromCookie()
    {
        if (!Request.Cookies.TryGetValue("seatLock", out var cookie))
        {
            return null;
        }

        try
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };
            return JsonSerializer.Deserialize<LockSeatCommandResponse>(cookie, options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
