namespace Chattt.Entities;

public class RefreshToken
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Token { get; set; }
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
    public DateTime? RevokedOn { get; set; }

    public bool IsActive => RevokedOn == null && DateTime.UtcNow < ExpiresOn;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}