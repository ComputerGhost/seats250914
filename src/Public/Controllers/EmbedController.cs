using Microsoft.AspNetCore.Mvc;

namespace Public.Controllers;

[Route("embed")]
public class EmbedController : Controller
{
    [HttpGet("kakao-map")]
    public IActionResult KakaoMap()
    {
        return View();
    }
}
