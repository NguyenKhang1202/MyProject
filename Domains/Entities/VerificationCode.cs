namespace MyProject.Domain;

public class VerificationCode : BaseEntity
{
    public int CodeId { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    
    public virtual User User { get; set; }
}