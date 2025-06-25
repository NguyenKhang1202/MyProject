using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyProject.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class SecureController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("You are authenticated!");
}
