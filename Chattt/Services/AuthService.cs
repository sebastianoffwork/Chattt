using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Chattt.Data;
using Chattt.DTOs;
using Chattt.Entities;
using Chattt.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Chattt.Services;

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Username == dto.Username))
        {
            return new AuthResult(false, Error: "Username already exists");
        }

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return new AuthResult(false, Error: "Invalid credentials");
        }

        return await GenerateTokensAsync(user);
    }

    public async Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await db.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (storedToken is null || !storedToken.IsActive)
        {
            return new AuthResult(false, Error: "Invalid refresh token");
        }

        storedToken.RevokedOn = DateTime.UtcNow;
        return await GenerateTokensAsync(storedToken.User);
    }

    private async Task<AuthResult> GenerateTokensAsync(User user)
    {
        var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var accessToken = new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityTokenHandler().CreateToken(tokenDescriptor)
        );

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresOn = DateTime.UtcNow.AddDays(30),
            UserId = user.Id
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        return new AuthResult(true, AccessToken: accessToken, RefreshToken: refreshToken.Token);
    }
}