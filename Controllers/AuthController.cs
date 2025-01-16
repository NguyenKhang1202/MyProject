using Microsoft.AspNetCore.Mvc;
using MyProject.Domain;
using MyProject.Domain.Dtos.Auths;
using MyProject.Domain.ErrorHandling;
using MyProject.Helpers;
using MyProject.Repos;

namespace MyProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration configuration, IUserRepo userRepo) : ControllerBase
{
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] LoginRequest loginRequest)
    {
        var user = await userRepo.FirstOrDefaultAsync(x => x.Username == loginRequest.Username);
        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }
        bool verifyPassword = Crypto.VerifyPassword(loginRequest.Password, user.PasswordHash);
        if (!verifyPassword)
        {
            return Unauthorized("Invalid username or password.");
        }
        
        var token = Generator.GenerateJwtToken(loginRequest.Username, configuration);
        return Ok(new { Token = token });
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        List<ErrorMessage> errors = new List<ErrorMessage>();
        var existingUser = await userRepo.FirstOrDefaultAsync(x => x.Username == registerRequest.Username);

        if (existingUser != null)
        {
            errors.Add(new ErrorMessage()
            {
                Code = 400,
                Message = "Username already exists."
            });
        }

        var user = new User
        {
            Username = registerRequest.Username,
            Email = registerRequest.Email,
            PasswordHash = Crypto.HashPassword(registerRequest.Password),
            DateOfBirth = registerRequest.DateOfBirth
        };

        if (errors.Count != 0)
        {
            return BadRequest(errors);
        }
        
        await userRepo.AddAsync(user);
        await userRepo.SaveChangesAsync();

        return Ok("User registered successfully.");
    }
}
