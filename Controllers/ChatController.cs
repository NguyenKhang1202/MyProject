using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Domain.Dtos.Chats;
using MyProject.Helpers;
using MyProject.Services;

namespace MyProject.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController(ILogger<ChatController> logger, IChatRoomService chatRoomService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateAsync(CreateChatRoomDto input)
    {
        return await ControllerHelper.TryCatchAsync(this, "CreateAsync", async () =>
        {
            await chatRoomService.CreateChatRoom(input);
            return Ok();
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        return await ControllerHelper.TryCatchAsync(this, "GetAsync", async () =>
        {
            await chatRoomService.GetChatRooms();
            return Ok();
        });
    }
}