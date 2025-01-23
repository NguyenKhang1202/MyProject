using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Domain.Dtos.Chats;
using MyProject.Helpers;
using MyProject.Services;

namespace MyProject.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController(ILogger<ChatController> logger, IMessageService messageService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(SendMessageDto input)
    {
        return await ControllerHelper.TryCatchAsync(this, "SendMessage", async () =>
        {
            await messageService.SendMessageAsync(input);
            return Ok();
        });
    }

    [HttpGet("{chatRoomId:int}")]
    public async Task<IActionResult> GetMessages(int chatRoomId)
    {
        return await ControllerHelper.TryCatchAsync(this, "GetMessages", async () =>
        {
            var result = await messageService.GetAsync(chatRoomId);
            return Ok(result);
        });
    }
}