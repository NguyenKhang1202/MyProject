using Microsoft.AspNetCore.Mvc;
using MyProject.Domain.Emails;
using MyProject.Services;

namespace MyProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController(EmailService emailService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        await emailService.SendEmailAsync(request);
        return Ok("Email sent successfully!");
    }
}
