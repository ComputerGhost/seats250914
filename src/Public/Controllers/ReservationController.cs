using Microsoft.AspNetCore.Mvc;

namespace Public.Controllers;

[Route("reservation")]
public class ReservationController : Controller
{
    [HttpGet("new")]
    public IActionResult ReserveSeat()
    {
        return View();
    }

    [HttpGet("payment")]
    public IActionResult MakePayment()
    {
        return View();
    }
}
