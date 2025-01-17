using MyProject.Context;
using MyProject.Domain;
using MyProject.Domain.Dtos.Auths;
using MyProject.Domain.Emails;
using MyProject.Domain.ErrorHandling;
using MyProject.Helpers;
using MyProject.Repos;

namespace MyProject.Services;

public interface IAuthService
{
    Task<List<ErrorMessage>> Register(RegisterRequest registerRequest);
    Task<List<ErrorMessage>> VerifyCodeAsync(string code, string email);
    Task<object> SignIn(LoginRequest loginRequest);
}

public class AuthService(
    IUserRepo userRepo, 
    IVerificationCodeRepo verificationCodeRepo, 
    EmailService emailService, 
    IHttpContextAccessor httpContext,
    IConfiguration configuration,
    MyDbContext dbContext): IAuthService
{
    public async Task<object> SignIn(LoginRequest loginRequest)
    {
        var errors = new List<ErrorMessage>();
        var user = await userRepo.FirstOrDefaultAsync(x => x.Username == loginRequest.Username);
        if (user == null)
        {
            errors.Add(new ErrorMessage()
            {
                Code = 400,
                Message = "Invalid username or password."
            });
        }
        bool verifyPassword = Crypto.VerifyPassword(loginRequest.Password, user.PasswordHash);
        if (!verifyPassword)
        {
            errors.Add(new ErrorMessage()
            {
                Code = 400,
                Message = "Invalid username or password."
            });
        }

        if (errors.Count == 0)
        {
            var token = Generator.GenerateJwtToken(user, configuration);
            return new { Token = token };
        }

        return errors;
    }
    public async Task<List<ErrorMessage>> Register(RegisterRequest registerRequest)
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

        if (errors.Count == 0)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await userRepo.AddAsync(user);
            await userRepo.SaveChangesAsync();
            var verificationCode = Generator.GenerateVerificationCode();
            await CreateVerificationCodeAsync(user.Id, verificationCode, TimeSpan.FromMinutes(5));
            await userRepo.SaveChangesAsync();
            await transaction.CommitAsync();
            await emailService.SendEmailAsync(new EmailRequest
            {
                ToEmail = registerRequest.Email,
                Subject = "Your Verification Code",
                Body = $"<p>Your verification code is: <strong>{verificationCode}</strong></p>",
            });
        }

        return errors;
    }
    
    private async Task CreateVerificationCodeAsync(int userId, string code, TimeSpan validDuration)
    {
        var verificationCode = new VerificationCode
        {
            UserId = userId,
            Code = code,
            ExpiresAt = DateTime.UtcNow.Add(validDuration),
            IsUsed = false
        };

        await verificationCodeRepo.AddAsync(verificationCode);
    }
    
    public async Task<List<ErrorMessage>> VerifyCodeAsync(string code, string email)
    {
        var errors = new List<ErrorMessage>();
        var user = await userRepo.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null)
        {
            errors.Add(new ErrorMessage()
            {
                Code = 401,
                Message = "User not found."
            });
        }
        
        var verificationCode = await verificationCodeRepo
            .FirstOrDefaultAsync(vc =>
                vc.UserId == user!.Id &&
                vc.Code == code &&
                vc.ExpiresAt > DateTime.UtcNow &&
                !vc.IsUsed);

        if (verificationCode == null)
        {
            errors.Add(new ErrorMessage()
            {
                Code = 400,
                Message = "Invalid verification code."
            });
        }

        if (errors.Count == 0)
        {
            verificationCode!.IsUsed = true;
            verificationCodeRepo.Update(verificationCode);
            user!.IsVerified = true;
            userRepo.Update(user);
            await verificationCodeRepo.SaveChangesAsync();
        }
        
        return errors;
    }

    private string? GetTokenClaimValue(string claimType)
    {
        var currentUser = httpContext?.HttpContext?.User;

        if (currentUser!.HasClaim(c => c.Type == claimType))
            return currentUser.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;

        return null;
    }
}