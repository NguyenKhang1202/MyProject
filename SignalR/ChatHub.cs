using Microsoft.AspNetCore.SignalR;

namespace MyProject.SignalR;

// Các hàm trong này để bên FE gọi, không phải BE dùng
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Welcome {Context.ConnectionId}!");
        await base.OnConnectedAsync();
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task SendMessage(string roomId, string user, string message)
    {
        await Clients.GroupExcept(roomId, new[] { Context.ConnectionId }).SendAsync("ReceiveMessage", user, message);
    }
}