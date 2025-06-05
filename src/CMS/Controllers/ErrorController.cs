using Microsoft.AspNetCore.Mvc;

namespace CMS.Controllers;
public class ErrorController : Controller
{
    [Route("Error")]
    public IActionResult Error()
    {
        return View("ServerError");
    }

    [Route("Error/{statusCode}")]
    public IActionResult HandleErrorCode(int statusCode)
    {
        return statusCode switch
        {
            404 => View("NotFound"),
            500 => View("ServerError"),
            _ => View("OtherError")
        };
    }
}
