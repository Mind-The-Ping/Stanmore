using Microsoft.AspNetCore.Mvc;

namespace Stanmore.API.Controllers;

[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("API is live!");
}
