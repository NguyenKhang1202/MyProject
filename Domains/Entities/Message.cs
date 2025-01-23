namespace MyProject.Domain;

public class Message : BaseEntity
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
}