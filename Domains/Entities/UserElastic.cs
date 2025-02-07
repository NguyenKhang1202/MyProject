namespace MyProject.Domain;

public class UserElastic
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public string Address { get; set; }
    
    public DateTime CreatedAt { get; set; }
}