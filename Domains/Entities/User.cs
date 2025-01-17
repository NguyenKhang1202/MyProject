using System.Text.Json.Serialization;

namespace MyProject.Domain
{
    public class User : BaseEntity
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public string PasswordHash { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        
        public virtual ICollection<VerificationCode> VerificationCodes { get; set; }
    }
}
