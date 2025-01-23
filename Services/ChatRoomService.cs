using MyProject.Constants;
using MyProject.Domain;
using MyProject.Domain.Dtos.Chats;
using MyProject.Repos;

namespace MyProject.Services;

public interface IChatRoomService
{
    Task<ChatRoom> CreateChatRoom(CreateChatRoomDto input);
    Task<ICollection<ChatRoom>> GetChatRooms();
}

public class ChatRoomService(IChatRoomRepo chatRoomRepo, IAuthService authService) : IChatRoomService
{
    public async Task<ChatRoom> CreateChatRoom(CreateChatRoomDto input)
    {
        var userId = authService.GetTokenClaimValue(ClaimConstants.UserId);
        var chatRoom = new ChatRoom()
        {
            Name = input.Name,
            CreatedBy = int.Parse(userId!)
        };
        await chatRoomRepo.AddAsync(chatRoom);
        await chatRoomRepo.SaveChangesAsync();

        return chatRoom;
    }

    public async Task<ICollection<ChatRoom>> GetChatRooms()
    {
        var chatRooms = await chatRoomRepo.GetAllAsync();
        return chatRooms;
    }
}