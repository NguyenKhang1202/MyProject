using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MyProject.Constants;
using MyProject.Context;
using MyProject.Domain;
using MyProject.Domain.Dtos.Auths;
using MyProject.Helpers;
using MyProject.Repos;
using MyProject.Services;

namespace MyProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration configuration, 
    IUserRepo userRepo, 
    IAuthService authService, 
    IExternalLoginRepo externalLoginRepo, 
    MyDbContext myDbContext,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn([FromBody] LoginRequest loginRequest)
    {
        return await ControllerHelper.TryCatchAsync(this, "SignIn", async () =>
        {
            // example
            logger.LogInformation($"{loginRequest.Username} is login");
            var result = await authService.SignIn(loginRequest);
            if (result.IsSuccess is false)
            {
                return BadRequest(result.ErrorMessages);
            }

            return Ok(result.Data);
        });
    }

    [HttpGet("login-github")]
    public async Task<IActionResult> SignInGithub()
    {
        // Trigger GitHub OAuth process
        var redirectUrl = Url.Action(nameof(Callback), "Auth");
        return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, "GitHub");
    }

    [HttpGet("signin-github")]
    public async Task<IActionResult> Callback()
    {
        // Ensure the user is authenticated
        var authenticateResult = await HttpContext.AuthenticateAsync();

        if (!authenticateResult.Succeeded)
        {
            return Unauthorized(new { message = "Authentication failed" });
        }
        
        var email = authenticateResult.Principal.FindFirst("email")?.Value;
        var user = await userRepo.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null)
        {
            await using var transaction = await myDbContext.Database.BeginTransactionAsync();
            user = new User
            {
                Username = authenticateResult.Principal.FindFirst("login")?.Value!,
                Email = email!,
                IsVerified = true,
                IsActive = true,
            };
            await userRepo.AddAsync(user);
            await userRepo.SaveChangesAsync();

            await externalLoginRepo.AddAsync(new ExternalLogin()
            {
                Provider = ProviderConstants.Github,
                ProviderKey = authenticateResult.Principal.FindFirst("id")?.Value!,
                UserId = user.Id
            });
            await userRepo.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        else
        {
            var externalLogin = await externalLoginRepo.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (externalLogin is null)
            {
                await externalLoginRepo.AddAsync(new ExternalLogin()
                {
                    Provider = ProviderConstants.Github,
                    ProviderKey = authenticateResult.Principal.FindFirst("id")?.Value!,
                    UserId = user.Id
                });
                await externalLoginRepo.SaveChangesAsync();
            }
        }
        
        return Ok(new LoginResponseDto()
        {
            Token = Generator.GenerateJwtToken(user, configuration),
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth
        });
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        return await ControllerHelper.TryCatchAsync(this, "Register", async () =>
        {
            var errors = await authService.Register(registerRequest);
            if (errors.Count != 0)
            {
                return BadRequest(errors);
            }

            return Ok("User registered successfully.");
        });
    }
    
    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode(string code, string email)
    {
        return await ControllerHelper.TryCatchAsync(this, "VerifyCode", async () =>
        {
            var result = await authService.VerifyCodeAsync(code, email);
            if (result.IsSuccess is false)
            {
                return BadRequest(result.ErrorMessages);
            }

            return Ok(result.Data);
        });
    }
}
