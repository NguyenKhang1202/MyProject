namespace MyProject.Domain.Dtos.Chats;

public class SendMessageDto
{
    public int ChatRoomId { get; set; }
    public string Content { get; set; }
}