namespace MyProject.Domain;

public class ChatRoom : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CreatedBy { get; set; }
}