namespace MyProject.Domain.Dtos.Auths;

public class LoginResponseDto
{
    public string Token { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
}