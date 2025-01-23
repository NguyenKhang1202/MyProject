namespace MyProject.Domain;

public class ExternalLogin : BaseEntity
{
    public int ExternalLoginId { get; set; }
    public int UserId { get; set; }
    public string Provider { get; set; }
    public string ProviderKey { get; set; }

    public User User { get; set; }
}