using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MyProject.Constants;
using MyProject.Domain;
using MyProject.Domain.Dtos.Chats;
using MyProject.Repos;
using MyProject.SignalR;

namespace MyProject.Services;

public interface IMessageService
{
    Task<List<Message>> GetAsync(int chatRoomId);
    Task SendMessageAsync(SendMessageDto input);
}

public class MessageService(IMessageRepo messageRepo, IAuthService authService, IHubContext<ChatHub> hubContext) : IMessageService
{
    public async Task<List<Message>> GetAsync(int chatRoomId)
    {
        var messages = await messageRepo
            .Where(m => m.ChatRoomId == chatRoomId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return messages;
    }
    
    public async Task SendMessageAsync(SendMessageDto input)
    {
        var userId = authService.GetTokenClaimValue(ClaimConstants.UserId);
        var message = new Message()
        {
            ChatRoomId = input.ChatRoomId,
            UserId = int.Parse(userId ?? "0"),
            Content = input.Content
        };
        await messageRepo.AddAsync(message);
        await messageRepo.SaveChangesAsync();

        // Gửi tin nhắn qua SignalR
        await hubContext.Clients.Group(message.ChatRoomId.ToString()).SendAsync("ReceiveMessage", message.UserId, message.Content);
    }
}