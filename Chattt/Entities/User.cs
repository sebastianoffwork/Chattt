using System.ComponentModel.DataAnnotations;

namespace Chattt.Entities;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    [MaxLength(50)]
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}