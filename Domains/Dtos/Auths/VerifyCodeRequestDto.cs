namespace MyProject.Domain.Dtos.Auths;

public class VerifyCodeRequestDto
{
    public string Code  { get; set; }
    public string Email { get; set; }
}