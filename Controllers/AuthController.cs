using Microsoft.AspNetCore.Mvc;
using MyProject.Domain;
using MyProject.Domain.Dtos.Auths;
using MyProject.Domain.ErrorHandling;
using MyProject.Helpers;
using MyProject.Repos;
using MyProject.Services;

namespace MyProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration configuration, IUserRepo userRepo, IAuthService authService) : ControllerBase
{
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] LoginRequest loginRequest)
    {
        var user = await userRepo.FirstOrDefaultAsync(x => x.Username == loginRequest.Username);
        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        if (user.IsVerified is false)
        {
            return Unauthorized("User is not verified.");
        }
        
        bool verifyPassword = Crypto.VerifyPassword(loginRequest.Password, user.PasswordHash);
        if (!verifyPassword)
        {
            return Unauthorized("Invalid username or password.");
        }
        
        var token = Generator.GenerateJwtToken(user, configuration);
        return Ok(new LoginResponseDto()
        {
            Token = token,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth
        });
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var errors = await authService.Register(registerRequest);
        if (errors.Count != 0)
        {
            return BadRequest(errors);
        }
        
        return Ok("User registered successfully.");
    }
    
    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode(string code, string email)
    {
        var result = await authService.VerifyCodeAsync(code, email);
        if (result.IsSuccess == false)
        {
            return BadRequest(result.ErrorMessages);
        }

        return Ok(result.Data);
    }
}
