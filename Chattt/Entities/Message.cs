using System.ComponentModel.DataAnnotations;

namespace Chattt.Entities;

public class Message
{
    public Guid Id { get; init; } = Guid.NewGuid();
    [Required]
    [MaxLength(2000)]
    public required string Content { get; set; }
    public DateTime SentAt { get; init; } = DateTime.UtcNow;

    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public Guid ReceiverId { get; set; }
    public User Receiver { get; set; } = null!;
}