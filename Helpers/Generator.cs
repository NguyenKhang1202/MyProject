using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.IdentityModel.Tokens;
using MyProject.Domain;

namespace MyProject.Helpers;

public static class Generator
{
    public static string GetGuid()
    {
        return new SequentialGuidValueGenerator().Next((EntityEntry) null).ToString();
    }

    public static string GetRandomCode()
    {
        Random random = new Random();
        return new string(Enumerable.Repeat<string>("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", 50).Select<string, char>((Func<string, char>) (s => s[random.Next(s.Length)])).ToArray<char>());
    }
    
    public static string GenerateJwtToken(string username, IConfiguration configuration)
    {
        var jwtKey = configuration.GetSection("Jwt").Get<JwtKeys>();
        var key = Encoding.UTF8.GetBytes(jwtKey?.Secret);
        var issuer = jwtKey?.Issuer;
        var audience = "admin";

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}